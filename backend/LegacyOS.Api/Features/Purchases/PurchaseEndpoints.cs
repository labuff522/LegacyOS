using System.Security.Claims;
using System.Text.Json;
using LegacyOS.Api.Data;
using LegacyOS.Api.Features.Enrollments;
using Microsoft.EntityFrameworkCore;

namespace LegacyOS.Api.Features.Purchases;

public static class PurchaseEndpoints
{
    public static RouteGroupBuilder MapPurchaseEndpoints(this IEndpointRouteBuilder app)
    {
        var portal = app.MapGroup("/portal/purchases").RequireAuthorization("CustomerOnly");
        portal.MapGet("/catalog", CatalogAsync);
        portal.MapGet("/", OrdersAsync);
        portal.MapPost("/checkout", CheckoutAsync);
        app.MapPost("/stripe/webhook", WebhookAsync).AllowAnonymous();
        return portal;
    }

    private static async Task<IResult> CatalogAsync(ClaimsPrincipal principal, LegacyOSDbContext db)
    {
        var familyId = await FamilyIdAsync(principal, db);
        if (familyId is null) return Results.Forbid();
        var organizationIds = db.FamilyOrganizations.Where(x => x.FamilyId == familyId && x.IsActive).Select(x => x.OrganizationId);
        var plans = await db.MembershipPlans.Where(x => x.IsActive && organizationIds.Contains(x.OrganizationId))
            .Select(x => new { x.Id, x.Name, x.MonthlyPrice, organizationName = x.Organization.Name }).ToListAsync();
        var products = await db.Products.Where(x => x.IsActive)
            .Select(x => new { x.Id, x.Name, x.Description, x.Price, productType = x.ProductType.ToString() }).ToListAsync();
        return Results.Ok(new { membershipPlans = plans, products });
    }

    private static async Task<IResult> CheckoutAsync(CheckoutRequest request, ClaimsPrincipal principal,
        LegacyOSDbContext db, StripeCheckoutService stripe, CancellationToken ct)
    {
        var familyId = await FamilyIdAsync(principal, db);
        if (familyId is null || !Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Results.Forbid();
        var user = await db.PortalUsers.AsNoTracking().SingleAsync(x => x.Id == userId, ct);
        var order = new PurchaseOrder { Id = Guid.NewGuid(), PortalUserId = userId, FamilyId = familyId.Value, CreatedOn = DateTime.UtcNow };
        string itemName;

        if (request.MembershipPlanId is Guid planId && request.AthleteId is Guid athleteId)
        {
            var plan = await db.MembershipPlans.SingleOrDefaultAsync(x => x.Id == planId && x.IsActive &&
                x.Organization.FamilyOrganizations.Any(fo => fo.FamilyId == familyId && fo.IsActive), ct);
            var athlete = await db.Athletes.SingleOrDefaultAsync(x => x.Id == athleteId && x.FamilyId == familyId, ct);
            if (plan is null || athlete is null) return Results.BadRequest(new { message = "The athlete or membership plan is unavailable." });
            if (!await db.UsaWrestlingVerifications.AnyAsync(x => x.AthleteId == athlete.Id, ct))
                return Results.BadRequest(new { message = "Enter the athlete's USA Wrestling membership number before purchasing a membership." });
            var enrollment = new Enrollment { Id = Guid.NewGuid(), AthleteId = athlete.Id, MembershipPlanId = plan.Id, IsActive = false, CreatedOn = DateTime.UtcNow };
            db.Enrollments.Add(enrollment);
            order.Kind = PurchaseKind.MembershipPlan; order.AthleteId = athlete.Id; order.MembershipPlanId = plan.Id;
            order.Enrollment = enrollment; order.EnrollmentId = enrollment.Id; order.Amount = plan.MonthlyPrice; itemName = plan.Name;
        }
        else if (request.ProductId is Guid productId && request.AthleteId is null && request.MembershipPlanId is null)
        {
            var product = await db.Products.SingleOrDefaultAsync(x => x.Id == productId && x.IsActive, ct);
            if (product is null) return Results.BadRequest(new { message = "The product is unavailable." });
            order.Kind = PurchaseKind.Product; order.ProductId = product.Id; order.Amount = product.Price; itemName = product.Name;
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

    private static async Task<IResult> WebhookAsync(HttpRequest request, IConfiguration config, LegacyOSDbContext db)
    {
        using var reader = new StreamReader(request.Body);
        var payload = await reader.ReadToEndAsync();
        var secret = config["Stripe:WebhookSecret"];
        if (string.IsNullOrWhiteSpace(secret) || !StripeWebhookVerifier.Verify(payload, request.Headers["Stripe-Signature"].ToString(), secret, DateTimeOffset.UtcNow))
            return Results.BadRequest();
        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement; var type = root.GetProperty("type").GetString();
        var session = root.GetProperty("data").GetProperty("object");
        if (!session.TryGetProperty("metadata", out var metadata) || !metadata.TryGetProperty("purchase_order_id", out var orderValue) ||
            !Guid.TryParse(orderValue.GetString(), out var orderId)) return Results.Ok();
        var order = await db.PurchaseOrders.Include(x => x.Enrollment).SingleOrDefaultAsync(x => x.Id == orderId);
        if (order is null || order.StripeCheckoutSessionId != session.GetProperty("id").GetString()) return Results.Ok();

        if (type is "checkout.session.completed" or "checkout.session.async_payment_succeeded")
        {
            var paymentStatus = session.TryGetProperty("payment_status", out var ps) ? ps.GetString() : null;
            if (paymentStatus is "paid" or "no_payment_required")
            {
                order.Status = PurchaseStatus.Completed; order.CompletedOn ??= DateTime.UtcNow;
                order.StripeCustomerId = StringProperty(session, "customer"); order.StripeSubscriptionId = StringProperty(session, "subscription");
                if (order.Enrollment is not null) { order.Enrollment.IsActive = true; order.Enrollment.StartDate = DateTime.UtcNow; }
            }
        }
        else if (type == "checkout.session.expired") order.Status = PurchaseStatus.Expired;
        else if (type == "checkout.session.async_payment_failed") order.Status = PurchaseStatus.Failed;
        await db.SaveChangesAsync(); return Results.Ok();
    }

    private static string? StringProperty(JsonElement value, string name) =>
        value.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

    private static async Task<Guid?> FamilyIdAsync(ClaimsPrincipal principal, LegacyOSDbContext db) =>
        Guid.TryParse(principal.FindFirstValue("guardian_id"), out var guardianId)
            ? await db.Guardians.Where(x => x.Id == guardianId && x.Family.IsActive).Select(x => (Guid?)x.FamilyId).SingleOrDefaultAsync()
            : null;
}

public record CheckoutRequest(Guid? MembershipPlanId, Guid? ProductId, Guid? AthleteId);
