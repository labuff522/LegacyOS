using System.Security.Claims;
using LegacyOS.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace LegacyOS.Api.Features.Sessions;

public static class SessionEndpoints
{
    public static RouteGroupBuilder MapSessionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/staff/sessions").RequireAuthorization("StaffOnly");
        group.MapGet("/roster", RosterAsync);
        group.MapGet("/athletes/{athleteId:guid}/ledger", LedgerAsync);
        group.MapPost("/athletes/{athleteId:guid}/check-in", CheckInAsync);
        group.MapPost("/athletes/{athleteId:guid}/packages", GrantPackageAsync);
        group.MapPost("/lots/{lotId:guid}/adjust", AdjustAsync);
        group.MapGet("/products", async (LegacyOSDbContext db) => Results.Ok(await db.Products
            .Where(x => x.IsActive && x.IsSessionPackage).OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name, x.Price, x.HasUnlimitedSessions, x.SessionCount, x.ValidityDays }).ToListAsync()));
        return group;
    }

    private static async Task<IResult> RosterAsync(LegacyOSDbContext db)
    {
        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);
        var athletes = await db.Athletes.OrderBy(x => x.LastName).ThenBy(x => x.FirstName)
            .Select(x => new
            {
                x.Id, x.FirstName, x.LastName, x.FamilyId, x.Family.FamilyName,
                usaWrestling = db.UsaWrestlingVerifications.Where(v => v.AthleteId == x.Id)
                    .OrderByDescending(v => v.SubmittedOn)
                    .Select(v => new { status = v.Status.ToString(), v.MembershipNumber, v.SubmittedOn, v.ExpiresOn,
                        isExpired = v.Status == LegacyOS.Api.Features.UsaWrestling.UsaWrestlingVerificationStatus.Current && v.ExpiresOn != null && v.ExpiresOn < today })
                    .FirstOrDefault(),
                missingRequiredWaivers = db.WaiverTemplates.Count(w => w.IsActive && w.IsRequired &&
                    !db.WaiverSignatures.Any(s => s.WaiverTemplateId == w.Id && s.AthleteId == x.Id && s.ExpiresOn > now)),
                pendingPurchaseCount = db.PurchaseOrders.Count(o => o.AthleteId == x.Id && o.Status == Features.Purchases.PurchaseStatus.Pending),
                Packages = db.SessionCreditLots.Where(l => l.AthleteId == x.Id && l.IsActive)
                    .OrderBy(l => l.ExpiresOn).Select(l => new { l.Id, productName = l.Product.Name,
                        l.IsUnlimited, l.SessionsRemaining, l.GrantedOn, l.ExpiresOn,
                        isPaymentCurrent = l.PurchaseOrder == null || l.PurchaseOrder.IsPaymentCurrent,
                        isExpired = l.ExpiresOn <= now }).ToList()
            }).ToListAsync();
        return Results.Ok(athletes);
    }

    private static async Task<IResult> LedgerAsync(Guid athleteId, LegacyOSDbContext db)
    {
        if (!await db.Athletes.AnyAsync(x => x.Id == athleteId)) return Results.NotFound();
        return Results.Ok(await db.SessionLedgerEntries.Where(x => x.AthleteId == athleteId)
            .OrderByDescending(x => x.CreatedOn).Select(x => new { x.Id, entryType = x.EntryType.ToString(),
                x.SessionChange, x.Note, x.CreatedOn, productName = x.SessionCreditLot.Product.Name }).ToListAsync());
    }

    private static async Task<IResult> CheckInAsync(Guid athleteId, CheckInRequest request, ClaimsPrincipal principal, LegacyOSDbContext db)
    {
        var now = DateTime.UtcNow;
        var missingWaivers = await db.WaiverTemplates.CountAsync(w => w.IsActive && w.IsRequired &&
            !db.WaiverSignatures.Any(s => s.WaiverTemplateId == w.Id && s.AthleteId == athleteId && s.ExpiresOn > now));
        var today = DateOnly.FromDateTime(now);
        var usaCurrent = await db.UsaWrestlingVerifications.AnyAsync(v => v.AthleteId == athleteId &&
            v.Status == LegacyOS.Api.Features.UsaWrestling.UsaWrestlingVerificationStatus.Current && (v.ExpiresOn == null || v.ExpiresOn >= today));
        var paymentCurrent = !await db.SessionCreditLots.AnyAsync(l => l.AthleteId == athleteId && l.IsActive && l.PurchaseOrder != null && !l.PurchaseOrder.IsPaymentCurrent);
        var eligibilityIssues = new List<string>();
        if (missingWaivers > 0) eligibilityIssues.Add($"{missingWaivers} required waiver(s) unsigned");
        if (!usaCurrent) eligibilityIssues.Add("USA Wrestling membership not current");
        if (!paymentCurrent) eligibilityIssues.Add("payment plan overdue");
        if (eligibilityIssues.Count > 0 && !request.OverrideEligibility)
            return Results.Conflict(new { message = $"Check-in blocked: {string.Join(", ", eligibilityIssues)}." });
        if (eligibilityIssues.Count > 0 && string.IsNullOrWhiteSpace(request.OverrideReason))
            return Results.BadRequest(new { message = "A staff override reason is required." });
        var lots = await db.SessionCreditLots.Include(x => x.Product)
            .Where(x => x.AthleteId == athleteId && x.IsActive && x.ExpiresOn > now &&
                (x.IsUnlimited || x.SessionsRemaining > 0)).OrderBy(x => x.ExpiresOn).ToListAsync();
        if (lots.Count == 0) return Results.Conflict(new { message = "This athlete has no active session package." });
        var lot = lots.FirstOrDefault(x => !x.IsUnlimited) ?? lots[0];
        if (!lot.IsUnlimited) lot.SessionsRemaining!--;
        Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var staffId);
        db.SessionLedgerEntries.Add(new SessionLedgerEntry { Id = Guid.NewGuid(), SessionCreditLotId = lot.Id,
            AthleteId = athleteId, StaffPortalUserId = staffId, EntryType = SessionLedgerEntryType.CheckIn,
            SessionChange = lot.IsUnlimited ? 0 : -1,
            Note = eligibilityIssues.Count > 0 ? $"ELIGIBILITY OVERRIDE ({string.Join(", ", eligibilityIssues)}): {request.OverrideReason?.Trim()}. {request.Note?.Trim()}" : request.Note?.Trim(), CreatedOn = now });
        await db.SaveChangesAsync();
        return Results.Ok(new { lot.Id, lot.IsUnlimited, lot.SessionsRemaining, lot.ExpiresOn });
    }

    private static async Task<IResult> GrantPackageAsync(Guid athleteId, GrantPackageRequest request,
        ClaimsPrincipal principal, LegacyOSDbContext db)
    {
        var athlete = await db.Athletes.SingleOrDefaultAsync(x => x.Id == athleteId);
        var product = await db.Products.SingleOrDefaultAsync(x => x.Id == request.ProductId && x.IsActive && x.IsSessionPackage);
        if (athlete is null || product is null || product.ValidityDays is null) return Results.BadRequest(new { message = "The athlete or session package is unavailable." });
        if (!Enum.TryParse<SessionGrantSource>(request.GrantSource, true, out var source) || source == SessionGrantSource.Stripe)
            return Results.BadRequest(new { message = "Choose PaidOutsideStripe or Complimentary." });
        if (!Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var staffId)) return Results.Forbid();
        var grantedOn = request.ActivationDate?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) ?? DateTime.UtcNow;
        var lot = new SessionCreditLot { Id = Guid.NewGuid(), AthleteId = athlete.Id, ProductId = product.Id,
            GrantedByStaffPortalUserId = staffId, GrantSource = source, IsUnlimited = product.HasUnlimitedSessions,
            SessionsGranted = product.HasUnlimitedSessions ? null : product.SessionCount,
            SessionsRemaining = product.HasUnlimitedSessions ? null : product.SessionCount,
            GrantedOn = grantedOn, ExpiresOn = grantedOn.AddDays(product.ValidityDays.Value), IsActive = true };
        db.SessionCreditLots.Add(lot);
        db.SessionLedgerEntries.Add(new SessionLedgerEntry { Id = Guid.NewGuid(), SessionCreditLot = lot,
            AthleteId = athlete.Id, StaffPortalUserId = staffId, EntryType = SessionLedgerEntryType.Grant,
            SessionChange = product.HasUnlimitedSessions ? 0 : product.SessionCount!.Value,
            Note = $"Staff assignment ({source}): {request.Note?.Trim()}", CreatedOn = DateTime.UtcNow });
        await db.SaveChangesAsync();
        return Results.Created($"/staff/sessions/lots/{lot.Id}", new { lot.Id, lot.ExpiresOn, lot.SessionsRemaining, lot.IsUnlimited });
    }

    private static async Task<IResult> AdjustAsync(Guid lotId, AdjustmentRequest request, ClaimsPrincipal principal, LegacyOSDbContext db)
    {
        if (request.SessionChange == 0) return Results.BadRequest(new { message = "Adjustment cannot be zero." });
        var lot = await db.SessionCreditLots.SingleOrDefaultAsync(x => x.Id == lotId);
        if (lot is null) return Results.NotFound();
        if (lot.IsUnlimited) return Results.BadRequest(new { message = "Unlimited packages do not have a numeric balance." });
        if (lot.SessionsRemaining + request.SessionChange < 0) return Results.BadRequest(new { message = "Adjustment would make the balance negative." });
        lot.SessionsRemaining += request.SessionChange;
        Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var staffId);
        db.SessionLedgerEntries.Add(new SessionLedgerEntry { Id = Guid.NewGuid(), SessionCreditLotId = lot.Id,
            AthleteId = lot.AthleteId, StaffPortalUserId = staffId, EntryType = SessionLedgerEntryType.Adjustment,
            SessionChange = request.SessionChange, Note = request.Note?.Trim(), CreatedOn = DateTime.UtcNow });
        await db.SaveChangesAsync(); return Results.NoContent();
    }
}

public record CheckInRequest(string? Note, bool OverrideEligibility = false, string? OverrideReason = null);
public record AdjustmentRequest(int SessionChange, string? Note);
public record GrantPackageRequest(Guid ProductId, string GrantSource, DateOnly? ActivationDate, string? Note);
