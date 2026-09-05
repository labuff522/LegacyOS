using System.Security.Claims;
using System.Text.Json;
using LegacyOS.Api.Data;
using LegacyOS.Api.Features.Enrollments;
using Microsoft.EntityFrameworkCore;
using LegacyOS.Api.Features.Sessions;
using LegacyOS.Api.Features.Discounts;
using LegacyOS.Api.Features.Portal;

namespace LegacyOS.Api.Features.Purchases;

public static class PurchaseEndpoints
{
    public static RouteGroupBuilder MapPurchaseEndpoints(this IEndpointRouteBuilder app)
    {
        var portal = app.MapGroup("/portal/purchases").RequireAuthorization("CustomerOnly");
        portal.MapGet("/catalog", CatalogAsync);
        portal.MapGet("/", OrdersAsync);
        portal.MapPost("/checkout", CheckoutAsync);
        portal.MapPost("/confirm", ConfirmAsync);
        app.MapPost("/staff/purchases/{orderId:guid}/reconcile", ReconcileAsync).RequireAuthorization("StaffOnly");
        app.MapPost("/staff/purchases/{orderId:guid}/refund", RefundAsync).RequireAuthorization("StaffOnly");
        app.MapPost("/stripe/webhook", WebhookAsync).AllowAnonymous();
        return portal;
    }

    private static async Task<IResult> CatalogAsync(ClaimsPrincipal principal, LegacyOSDbContext db)
    {
        var familyId = await FamilyIdAsync(principal, db);
        if (familyId is null) return Results.Forbid();
        var products = await db.Products.Where(x => x.IsActive)
            .Select(x => new { x.Id, x.Name, x.Description, x.Price, productType = x.ProductType.ToString(),
                x.IsSessionPackage, x.HasUnlimitedSessions, x.SessionCount, x.ValidityDays, x.InstallmentCount, x.BillingDayOfMonth }).ToListAsync();
        return Results.Ok(new { products });
    }

    private static async Task<IResult> CheckoutAsync(CheckoutRequest request, ClaimsPrincipal principal,
        LegacyOSDbContext db, StripeCheckoutService stripe, CancellationToken ct)
    {
        var familyId = await FamilyIdAsync(principal, db);
        if (familyId is null || !Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Results.Forbid();
        if (request.MembershipPlanId is not null)
            return Results.BadRequest(new { message = "Legacy membership plans are no longer available. Choose a product." });
        var user = await db.PortalUsers.AsNoTracking().SingleAsync(x => x.Id == userId, ct);
        var familySnapshot = await db.Families.Where(x => x.Id == familyId).Select(x => new { x.Id, x.FamilyName,
            guardians = x.Guardians.Select(g => new { g.Id, g.FirstName, g.LastName, g.Email, g.Phone }).ToList(),
            athletes = x.Athletes.Select(a => new { a.Id, a.FirstName, a.LastName, a.DateOfBirth, a.Gender }).ToList() }).SingleAsync(ct);
        var order = new PurchaseOrder { Id = Guid.NewGuid(), PortalUserId = userId, FamilyId = familyId.Value,
            FamilySnapshotJson = JsonSerializer.Serialize(familySnapshot), CreatedOn = DateTime.UtcNow };
        string itemName;

        if (request.MembershipPlanId is Guid planId && request.AthleteId is Guid athleteId)
        {
            var plan = await db.MembershipPlans.SingleOrDefaultAsync(x => x.Id == planId && x.IsActive &&
                x.Organization.FamilyOrganizations.Any(fo => fo.FamilyId == familyId && fo.IsActive), ct);
            var athlete = await db.Athletes.Include(x => x.AthleteGroup).SingleOrDefaultAsync(x => x.Id == athleteId && x.FamilyId == familyId, ct);
            if (plan is null || athlete is null) return Results.BadRequest(new { message = "The athlete or membership plan is unavailable." });
            if (!await db.UsaWrestlingVerifications.AnyAsync(x => x.AthleteId == athlete.Id, ct))
                return Results.BadRequest(new { message = "Enter the athlete's USA Wrestling membership number before purchasing a membership." });
            var enrollment = new Enrollment { Id = Guid.NewGuid(), AthleteId = athlete.Id, MembershipPlanId = plan.Id, IsActive = false, CreatedOn = DateTime.UtcNow };
            db.Enrollments.Add(enrollment);
            order.Kind = PurchaseKind.MembershipPlan; order.AthleteId = athlete.Id; order.MembershipPlanId = plan.Id;
            order.Enrollment = enrollment; order.EnrollmentId = enrollment.Id; order.Amount = plan.MonthlyPrice; order.OriginalAmount = plan.MonthlyPrice; itemName = plan.Name;
            order.AthleteSnapshotJson = JsonSerializer.Serialize(new { athlete.Id, athlete.FirstName, athlete.LastName, athlete.DateOfBirth, athlete.Gender, athleteGroup = athlete.AthleteGroup == null ? null : new { athlete.AthleteGroup.Id, athlete.AthleteGroup.Name, athlete.AthleteGroup.Description } });
            order.ItemSnapshotJson = JsonSerializer.Serialize(new { plan.Id, plan.Name, plan.ShortName, plan.MonthlyPrice, kind = "MembershipPlan" });
        }
        else if (request.ProductId is Guid productId && request.MembershipPlanId is null)
        {
            var product = await db.Products.SingleOrDefaultAsync(x => x.Id == productId && x.IsActive, ct);
            if (product is null) return Results.BadRequest(new { message = "The product is unavailable." });
            if (product.IsSessionPackage)
            {
                if (request.AthleteId is not Guid packageAthleteId ||
                    !await db.Athletes.AnyAsync(x => x.Id == packageAthleteId && x.FamilyId == familyId, ct))
                    return Results.BadRequest(new { message = "Choose an athlete in your family for this session package." });
                if (!await db.UsaWrestlingVerifications.AnyAsync(x => x.AthleteId == packageAthleteId, ct))
                    return Results.BadRequest(new { message = "Enter the athlete's USA Wrestling membership number before purchasing." });
                var missingWaiver = await db.WaiverTemplates.AnyAsync(w => w.IsActive && w.IsRequired &&
                    !db.WaiverSignatures.Any(s => s.WaiverTemplateId == w.Id && s.AthleteId == packageAthleteId && s.ExpiresOn > DateTime.UtcNow), ct);
                if (missingWaiver) return Results.BadRequest(new { message = "Sign every required waiver for this athlete before purchasing." });
                order.AthleteId = packageAthleteId;
                var athlete = await db.Athletes.AsNoTracking().Include(x => x.AthleteGroup).SingleAsync(x => x.Id == packageAthleteId, ct);
                order.AthleteSnapshotJson = JsonSerializer.Serialize(new { athlete.Id, athlete.FirstName, athlete.LastName, athlete.DateOfBirth, athlete.Gender, athleteGroup = athlete.AthleteGroup == null ? null : new { athlete.AthleteGroup.Id, athlete.AthleteGroup.Name, athlete.AthleteGroup.Description } });
            }
            else if (request.AthleteId is not null) return Results.BadRequest(new { message = "This product does not require an athlete." });
            order.Kind = PurchaseKind.Product; order.ProductId = product.Id; order.Amount = product.Price; order.OriginalAmount = product.Price; itemName = product.Name;
            order.InstallmentCount = product.InstallmentCount ?? 1;
            order.BillingDayOfMonth = product.BillingDayOfMonth;
            order.InstallmentAmount = decimal.Round(order.Amount / order.InstallmentCount, 2);
            order.ItemSnapshotJson = JsonSerializer.Serialize(new { product.Id, product.Name, product.ShortName, product.Description,
                product.ProductType, product.Price, product.IsSessionPackage, product.HasUnlimitedSessions, product.SessionCount, product.ValidityDays,
                product.InstallmentCount, product.BillingDayOfMonth });
            DiscountCode? discount = null;
            if (!string.IsNullOrWhiteSpace(request.DiscountCode))
            {
                var now = DateTime.UtcNow; var normalizedCode = request.DiscountCode.Trim().ToUpperInvariant();
                discount = await db.DiscountCodes.SingleOrDefaultAsync(x => x.Code == normalizedCode && !x.IsAutomaticSibling && x.IsActive &&
                    (x.ProductId == null || x.ProductId == product.Id) && (x.StartsOn == null || x.StartsOn <= now) &&
                    (x.EndsOn == null || x.EndsOn >= now) && (x.MaxRedemptions == null || x.RedemptionCount < x.MaxRedemptions), ct);
                if (discount is null) return Results.BadRequest(new { message = "The discount code is invalid or unavailable." });
            }
            else if (order.AthleteId is Guid siblingAthleteId)
            {
                var now = DateTime.UtcNow;
                var priorAthletes = await db.PurchaseOrders.Where(x => x.FamilyId == familyId && x.AthleteId != null &&
                        x.Status == PurchaseStatus.Completed && x.AthleteId != siblingAthleteId)
                    .GroupBy(x => x.AthleteId).Select(x => new { AthleteId = x.Key, FirstPurchase = x.Min(y => y.CompletedOn) })
                    .OrderBy(x => x.FirstPurchase).Select(x => x.AthleteId).ToListAsync(ct);
                var existingPosition = await db.PurchaseOrders.Where(x => x.FamilyId == familyId && x.AthleteId == siblingAthleteId && x.Status == PurchaseStatus.Completed)
                    .Select(x => (DateTime?)x.CompletedOn).MinAsync(ct);
                var siblingPosition = existingPosition is null ? priorAthletes.Count + 1 :
                    1 + await db.PurchaseOrders.Where(x => x.FamilyId == familyId && x.AthleteId != siblingAthleteId && x.AthleteId != null &&
                        x.Status == PurchaseStatus.Completed && x.CompletedOn < existingPosition).Select(x => x.AthleteId).Distinct().CountAsync(ct);
                discount = await db.DiscountCodes.Where(x => x.IsAutomaticSibling && x.IsActive &&
                        (x.ProductId == null || x.ProductId == product.Id) && (x.StartsOn == null || x.StartsOn <= now) &&
                        (x.EndsOn == null || x.EndsOn >= now) && (x.MaxRedemptions == null || x.RedemptionCount < x.MaxRedemptions) &&
                        siblingPosition >= x.SiblingStartPosition && siblingPosition <= x.SiblingEndPosition)
                    .OrderByDescending(x => x.DiscountType == DiscountType.Percentage ? product.Price * x.Value / 100m : x.Value).FirstOrDefaultAsync(ct);
            }
            if (discount is not null)
            {
                order.DiscountCodeId = discount.Id; order.DiscountCodeSnapshot = discount.Code;
                order.DiscountAmount = discount.DiscountType == DiscountType.Percentage
                    ? decimal.Round(product.Price * discount.Value / 100m, 2) : Math.Min(product.Price, discount.Value);
                order.Amount = Math.Max(0, product.Price - order.DiscountAmount);
                if (order.InstallmentCount > 1 && decimal.Round(order.Amount * 100m) % order.InstallmentCount != 0)
                    return Results.BadRequest(new { message = "This discount cannot be divided evenly across the configured payments." });
                order.InstallmentAmount = decimal.Round(order.Amount / order.InstallmentCount, 2);
            }
        }
        else return Results.BadRequest(new { message = "Choose one membership plan and athlete, or one product." });

        db.PurchaseOrders.Add(order); await db.SaveChangesAsync(ct);
        try
        {
            var session = await stripe.CreateAsync(order, itemName, user.Email, ct);
            order.StripeCheckoutSessionId = session.SessionId; await db.SaveChangesAsync(ct);
            return Results.Ok(new { orderId = order.Id, checkoutUrl = session.Url });
        }
        catch (InvalidOperationException ex)
        {
            order.Status = PurchaseStatus.Failed; await db.SaveChangesAsync(ct);
            return Results.Problem(ex.Message, statusCode: StatusCodes.Status502BadGateway);
        }
    }

    private static async Task<IResult> OrdersAsync(ClaimsPrincipal principal, LegacyOSDbContext db)
    {
        var familyId = await FamilyIdAsync(principal, db);
        if (familyId is null) return Results.Forbid();
        var orders = await db.PurchaseOrders.Where(x => x.FamilyId == familyId)
            .OrderByDescending(x => x.CreatedOn).Select(x => new { x.Id, kind = x.Kind.ToString(), status = x.Status.ToString(),
                x.Amount, x.Currency, x.CreatedOn, x.CompletedOn, itemName = x.MembershipPlan != null ? x.MembershipPlan.Name : x.Product!.Name }).ToListAsync();
        return Results.Ok(orders);
    }

    private static async Task<IResult> ConfirmAsync(ConfirmCheckoutRequest request, ClaimsPrincipal principal,
        LegacyOSDbContext db, StripeCheckoutService stripe, PasswordResetEmailService emailService,
        ILogger<PasswordResetEmailService> logger, CancellationToken ct)
    {
        var familyId = await FamilyIdAsync(principal, db);
        if (familyId is null || string.IsNullOrWhiteSpace(request.SessionId)) return Results.Forbid();
        var order = await db.PurchaseOrders.Include(x => x.Enrollment).Include(x => x.DiscountCode)
            .SingleOrDefaultAsync(x => x.FamilyId == familyId && x.StripeCheckoutSessionId == request.SessionId, ct);
        if (order is null) return Results.NotFound();
        if (order.Status == PurchaseStatus.Completed) { await TrySendPurchaseConfirmationAsync(order, db, emailService, logger, ct); return Results.Ok(new { status = order.Status.ToString() }); }
        var session = await stripe.GetAsync(request.SessionId, ct);
        if (session.SessionId != order.StripeCheckoutSessionId) return Results.Forbid();
        order.StripeCustomerId = session.CustomerId; order.StripeSubscriptionId = session.SubscriptionId; order.StripePaymentIntentId = session.PaymentIntentId;
        await stripe.SetFiniteEndAsync(order, ct);
        if (session.PaymentStatus == "paid" || (session.PaymentStatus == "no_payment_required" && session.SubscriptionId is not null))
            await CompleteOrderAsync(order, db);
        await db.SaveChangesAsync(ct);
        await TrySendPurchaseConfirmationAsync(order, db, emailService, logger, ct);
        return Results.Ok(new { status = order.Status.ToString() });
    }

    private static async Task<IResult> ReconcileAsync(Guid orderId, LegacyOSDbContext db, StripeCheckoutService stripe,
        PasswordResetEmailService emailService, ILogger<PasswordResetEmailService> logger, CancellationToken ct)
    {
        var order = await db.PurchaseOrders.Include(x => x.Enrollment).Include(x => x.DiscountCode).SingleOrDefaultAsync(x => x.Id == orderId, ct);
        if (order is null) return Results.NotFound();
        if (order.Status == PurchaseStatus.Completed) { await TrySendPurchaseConfirmationAsync(order, db, emailService, logger, ct); return Results.Ok(new { status = order.Status.ToString() }); }
        if (string.IsNullOrWhiteSpace(order.StripeCheckoutSessionId)) return Results.BadRequest(new { message = "This order has no Stripe Checkout Session." });
        var session = await stripe.GetAsync(order.StripeCheckoutSessionId, ct);
        order.StripeCustomerId = session.CustomerId; order.StripeSubscriptionId = session.SubscriptionId; order.StripePaymentIntentId = session.PaymentIntentId;
        await stripe.SetFiniteEndAsync(order, ct);
        if (session.PaymentStatus == "paid" || (session.PaymentStatus == "no_payment_required" && session.SubscriptionId is not null))
            await CompleteOrderAsync(order, db);
        await db.SaveChangesAsync(ct);
        await TrySendPurchaseConfirmationAsync(order, db, emailService, logger, ct);
        return Results.Ok(new { status = order.Status.ToString(), paymentStatus = session.PaymentStatus });
    }

    private static async Task<IResult> RefundAsync(Guid orderId, RefundOrderRequest request, ClaimsPrincipal principal,
        LegacyOSDbContext db, StripeCheckoutService stripe, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Reason) || !Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var staffId))
            return Results.BadRequest(new { message = "A staff refund reason is required." });
        var order = await db.PurchaseOrders.Include(x => x.SessionCreditLot).SingleOrDefaultAsync(x => x.Id == orderId, ct);
        if (order is null) return Results.NotFound();
        if (order.Status == PurchaseStatus.Refunded) return Results.Ok(new { status = order.Status.ToString() });
        if (order.Status != PurchaseStatus.Completed) return Results.BadRequest(new { message = "Only a completed order can be refunded." });
        if (string.IsNullOrWhiteSpace(order.StripePaymentIntentId) && !string.IsNullOrWhiteSpace(order.StripeCheckoutSessionId))
        {
            var session = await stripe.GetAsync(order.StripeCheckoutSessionId, ct);
            order.StripePaymentIntentId = session.PaymentIntentId; order.StripeSubscriptionId ??= session.SubscriptionId;
        }
        var result = await stripe.RefundAndCancelAsync(order.StripePaymentIntentId, order.StripeSubscriptionId, order.Id, ct);
        order.StripeRefundId = result.RefundId; order.Status = PurchaseStatus.Refunded; order.RefundedOn = DateTime.UtcNow;
        order.RefundedByPortalUserId = staffId; order.RefundReason = request.Reason.Trim(); order.IsPaymentCurrent = false;
        DeactivateRefundedPackage(order, db, staffId);
        await db.SaveChangesAsync(ct); return Results.Ok(new { status = order.Status.ToString() });
    }

    private static void DeactivateRefundedPackage(PurchaseOrder order, LegacyOSDbContext db, Guid? staffId)
    {
        if (order.SessionCreditLot is null || !order.SessionCreditLot.IsActive) return;
        var remaining = order.SessionCreditLot.IsUnlimited ? 0 : order.SessionCreditLot.SessionsRemaining ?? 0;
        order.SessionCreditLot.IsActive = false;
        db.SessionLedgerEntries.Add(new SessionLedgerEntry { Id = Guid.NewGuid(), SessionCreditLotId = order.SessionCreditLot.Id,
            AthleteId = order.SessionCreditLot.AthleteId, StaffPortalUserId = staffId, EntryType = SessionLedgerEntryType.Refund,
            SessionChange = -remaining, Note = $"Package deactivated after refund: {order.RefundReason}", CreatedOn = DateTime.UtcNow });
    }

    private static async Task<IResult> WebhookAsync(HttpRequest request, IConfiguration config, LegacyOSDbContext db,
        StripeCheckoutService stripe, PasswordResetEmailService emailService,
        ILogger<PasswordResetEmailService> logger, CancellationToken ct)
    {
        using var reader = new StreamReader(request.Body);
        var payload = await reader.ReadToEndAsync();
        var secret = config["Stripe:WebhookSecret"];
        if (string.IsNullOrWhiteSpace(secret) || !StripeWebhookVerifier.Verify(payload, request.Headers["Stripe-Signature"].ToString(), secret, DateTimeOffset.UtcNow))
            return Results.BadRequest();
        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement; var type = root.GetProperty("type").GetString();
        var session = root.GetProperty("data").GetProperty("object");
        if (type is "refund.created" or "refund.updated")
        {
            var paymentIntentId = StringProperty(session, "payment_intent");
            if (StringProperty(session, "status") == "succeeded" && paymentIntentId is not null)
            {
                var refundedOrder = await db.PurchaseOrders.Include(x => x.SessionCreditLot).SingleOrDefaultAsync(x => x.StripePaymentIntentId == paymentIntentId);
                if (refundedOrder is not null && refundedOrder.Status != PurchaseStatus.Refunded)
                {
                    refundedOrder.StripeRefundId = StringProperty(session, "id"); refundedOrder.Status = PurchaseStatus.Refunded;
                    refundedOrder.RefundedOn = DateTime.UtcNow; refundedOrder.RefundReason = "Refund confirmed by Stripe."; refundedOrder.IsPaymentCurrent = false;
                    DeactivateRefundedPackage(refundedOrder, db, null); await db.SaveChangesAsync();
                }
            }
            return Results.Ok();
        }
        if (type is "invoice.paid" or "invoice.payment_failed")
        {
            var subscriptionId = StringProperty(session, "subscription");
            var installmentOrder = await db.PurchaseOrders.SingleOrDefaultAsync(x => x.StripeSubscriptionId == subscriptionId);
            if (installmentOrder is null) return Results.Ok();
            installmentOrder.IsPaymentCurrent = type == "invoice.paid";
            if (type == "invoice.paid")
            {
                installmentOrder.Status = PurchaseStatus.Completed; installmentOrder.CompletedOn ??= DateTime.UtcNow;
                await GrantPackageAsync(installmentOrder, db);
            }
            await db.SaveChangesAsync(); await TrySendPurchaseConfirmationAsync(installmentOrder, db, emailService, logger, ct); return Results.Ok();
        }
        if (!session.TryGetProperty("metadata", out var metadata) || !metadata.TryGetProperty("purchase_order_id", out var orderValue) ||
            !Guid.TryParse(orderValue.GetString(), out var orderId)) return Results.Ok();
        var order = await db.PurchaseOrders.Include(x => x.Enrollment).Include(x => x.DiscountCode).SingleOrDefaultAsync(x => x.Id == orderId);
        if (order is null || order.StripeCheckoutSessionId != session.GetProperty("id").GetString()) return Results.Ok();

        if (type is "checkout.session.completed" or "checkout.session.async_payment_succeeded")
        {
            var paymentStatus = session.TryGetProperty("payment_status", out var ps) ? ps.GetString() : null;
            order.StripeCustomerId = StringProperty(session, "customer"); order.StripeSubscriptionId = StringProperty(session, "subscription");
            await stripe.SetFiniteEndAsync(order, ct);
            if (paymentStatus == "paid" || (paymentStatus == "no_payment_required" && order.StripeSubscriptionId is not null))
            {
                await CompleteOrderAsync(order, db);
            }
        }
        else if (type == "checkout.session.expired") order.Status = PurchaseStatus.Expired;
        else if (type == "checkout.session.async_payment_failed") order.Status = PurchaseStatus.Failed;
        await db.SaveChangesAsync(); await TrySendPurchaseConfirmationAsync(order, db, emailService, logger, ct); return Results.Ok();
    }

    private static string? StringProperty(JsonElement value, string name) =>
        value.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

    private static async Task<Guid?> FamilyIdAsync(ClaimsPrincipal principal, LegacyOSDbContext db) =>
        Guid.TryParse(principal.FindFirstValue("guardian_id"), out var guardianId)
            ? await db.Guardians.Where(x => x.Id == guardianId && x.Family.IsActive).Select(x => (Guid?)x.FamilyId).SingleOrDefaultAsync()
            : null;

    private static async Task GrantPackageAsync(PurchaseOrder order, LegacyOSDbContext db)
    {
        if (order.ProductId is not Guid productId || order.AthleteId is not Guid athleteId ||
            await db.SessionCreditLots.AnyAsync(x => x.PurchaseOrderId == order.Id)) return;
        var product = await db.Products.SingleAsync(x => x.Id == productId);
        if (!product.IsSessionPackage || product.ValidityDays is not int validityDays) return;
        var grantedOn = order.CompletedOn ?? DateTime.UtcNow;
        var lot = new SessionCreditLot { Id = Guid.NewGuid(), AthleteId = athleteId, ProductId = product.Id,
            PurchaseOrderId = order.Id, IsUnlimited = product.HasUnlimitedSessions, GrantSource = SessionGrantSource.Stripe,
            SessionsGranted = product.HasUnlimitedSessions ? null : product.SessionCount,
            SessionsRemaining = product.HasUnlimitedSessions ? null : product.SessionCount,
            GrantedOn = grantedOn, ExpiresOn = grantedOn.AddDays(validityDays), IsActive = true };
        db.SessionCreditLots.Add(lot);
        db.SessionLedgerEntries.Add(new SessionLedgerEntry { Id = Guid.NewGuid(), SessionCreditLot = lot,
            AthleteId = athleteId, EntryType = SessionLedgerEntryType.Grant,
            SessionChange = product.HasUnlimitedSessions ? 0 : product.SessionCount!.Value,
            Note = $"Granted by completed purchase {order.Id}." });
    }

    private static async Task CompleteOrderAsync(PurchaseOrder order, LegacyOSDbContext db)
    {
        order.Status = PurchaseStatus.Completed; order.CompletedOn ??= DateTime.UtcNow;
        if (order.DiscountCode is not null && !order.DiscountRedemptionRecorded)
        { order.DiscountCode.RedemptionCount++; order.DiscountRedemptionRecorded = true; }
        if (order.Enrollment is not null) { order.Enrollment.IsActive = true; order.Enrollment.StartDate = DateTime.UtcNow; }
        await GrantPackageAsync(order, db);
    }

    private static async Task TrySendPurchaseConfirmationAsync(PurchaseOrder order, LegacyOSDbContext db,
        PasswordResetEmailService emailService, ILogger logger, CancellationToken ct)
    {
        if (order.Status != PurchaseStatus.Completed || order.PurchaseConfirmationSentOn is not null) return;
        try
        {
            var email = await db.PortalUsers.Where(x => x.Id == order.PortalUserId).Select(x => x.Email).SingleAsync(ct);
            var itemName = SnapshotValue(order.ItemSnapshotJson, "Name") ?? "DenOS purchase";
            var athleteName = SnapshotValue(order.AthleteSnapshotJson, "FirstName") is string first
                ? $"{first} {SnapshotValue(order.AthleteSnapshotJson, "LastName")}".Trim() : null;
            await emailService.SendPurchaseConfirmationAsync(email, itemName, athleteName, order.Amount, order.Currency, order.Id, ct);
            order.PurchaseConfirmationSentOn = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        catch (Exception exception) { logger.LogError(exception, "Purchase confirmation email failed for order {OrderId}.", order.Id); }
    }

    private static string? SnapshotValue(string? json, string name)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        using var document = JsonDocument.Parse(json);
        foreach (var property in document.RootElement.EnumerateObject())
            if (property.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && property.Value.ValueKind == JsonValueKind.String) return property.Value.GetString();
        return null;
    }
}

public record CheckoutRequest(Guid? MembershipPlanId, Guid? ProductId, Guid? AthleteId, string? DiscountCode = null);
public record ConfirmCheckoutRequest(string SessionId);
public record RefundOrderRequest(string Reason);
