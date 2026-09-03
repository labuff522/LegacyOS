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
        group.MapPost("/lots/{lotId:guid}/adjust", AdjustAsync);
        return group;
    }

    private static async Task<IResult> RosterAsync(LegacyOSDbContext db)
    {
        var now = DateTime.UtcNow;
        var athletes = await db.Athletes.OrderBy(x => x.LastName).ThenBy(x => x.FirstName)
            .Select(x => new
            {
                x.Id, x.FirstName, x.LastName, x.FamilyId, x.Family.FamilyName,
                Packages = db.SessionCreditLots.Where(l => l.AthleteId == x.Id && l.IsActive)
                    .OrderBy(l => l.ExpiresOn).Select(l => new { l.Id, productName = l.Product.Name,
                        l.IsUnlimited, l.SessionsRemaining, l.GrantedOn, l.ExpiresOn,
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
        var lots = await db.SessionCreditLots.Include(x => x.Product)
            .Where(x => x.AthleteId == athleteId && x.IsActive && x.ExpiresOn > now &&
                (x.IsUnlimited || x.SessionsRemaining > 0)).OrderBy(x => x.ExpiresOn).ToListAsync();
        if (lots.Count == 0) return Results.Conflict(new { message = "This athlete has no active session package." });
        var lot = lots.FirstOrDefault(x => !x.IsUnlimited) ?? lots[0];
        if (!lot.IsUnlimited) lot.SessionsRemaining!--;
        Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var staffId);
        db.SessionLedgerEntries.Add(new SessionLedgerEntry { Id = Guid.NewGuid(), SessionCreditLotId = lot.Id,
            AthleteId = athleteId, StaffPortalUserId = staffId, EntryType = SessionLedgerEntryType.CheckIn,
            SessionChange = lot.IsUnlimited ? 0 : -1, Note = request.Note?.Trim(), CreatedOn = now });
        await db.SaveChangesAsync();
        return Results.Ok(new { lot.Id, lot.IsUnlimited, lot.SessionsRemaining, lot.ExpiresOn });
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

public record CheckInRequest(string? Note);
public record AdjustmentRequest(int SessionChange, string? Note);
