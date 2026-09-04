using System.Security.Claims;
using System.Text.RegularExpressions;
using LegacyOS.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace LegacyOS.Api.Features.UsaWrestling;

public static partial class UsaWrestlingEndpoints
{
    public static void MapUsaWrestlingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPut("/portal/athletes/{athleteId:guid}/usa-wrestling-membership", SubmitAsync).RequireAuthorization("CustomerOnly");
        var staff = app.MapGroup("/staff/usa-wrestling-verifications").RequireAuthorization("StaffOnly");
        staff.MapGet("/", PendingAsync); staff.MapPut("/{id:guid}", ReviewAsync);
    }

    private static async Task<IResult> SubmitAsync(Guid athleteId, SubmitUsaWrestlingRequest request, ClaimsPrincipal principal, LegacyOSDbContext db)
    {
        var number = request.MembershipNumber.Trim().ToUpperInvariant();
        if (!MembershipNumberPattern().IsMatch(number))
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["membershipNumber"] = ["Enter a membership number using 3–50 letters, numbers, or hyphens."] });
        if (!Guid.TryParse(principal.FindFirstValue("guardian_id"), out var guardianId) ||
            !await db.Athletes.AnyAsync(x => x.Id == athleteId && x.Family.Guardians.Any(g => g.Id == guardianId))) return Results.Forbid();
        if (await db.UsaWrestlingVerifications.AnyAsync(x => x.MembershipNumber == number && x.AthleteId != athleteId))
            return Results.Conflict(new { message = "That membership number is already assigned to another athlete." });
        var verification = await db.UsaWrestlingVerifications.SingleOrDefaultAsync(x => x.AthleteId == athleteId);
        if (verification is null) { verification = new() { Id = Guid.NewGuid(), AthleteId = athleteId }; db.Add(verification); }
        verification.MembershipNumber = number; verification.Status = UsaWrestlingVerificationStatus.Pending;
        verification.SubmittedOn = DateTime.UtcNow; verification.VerifiedOn = null; verification.ExpiresOn = null;
        verification.VerifiedByPortalUserId = null; verification.StaffNotes = null;
        await db.SaveChangesAsync(); return Results.Ok(ToDto(verification));
    }

    private static async Task<IResult> PendingAsync(LegacyOSDbContext db)
    {
        var rows = await db.UsaWrestlingVerifications.Include(x => x.Athlete).ThenInclude(x => x.Family)
            .Where(x => x.Status != UsaWrestlingVerificationStatus.Current && x.Status != UsaWrestlingVerificationStatus.Expired)
            .OrderBy(x => x.Status != UsaWrestlingVerificationStatus.Pending).ThenBy(x => x.SubmittedOn)
            .Select(x => new { x.Id, x.AthleteId, athleteName = x.Athlete.FirstName + " " + x.Athlete.LastName,
                x.Athlete.Family.FamilyName, x.MembershipNumber, status = x.Status.ToString(), x.SubmittedOn, x.ExpiresOn, x.StaffNotes }).ToListAsync();
        return Results.Ok(rows);
    }

    private static async Task<IResult> ReviewAsync(Guid id, ReviewUsaWrestlingRequest request, ClaimsPrincipal principal, LegacyOSDbContext db)
    {
        if (!Enum.TryParse<UsaWrestlingVerificationStatus>(request.Status, true, out var status) || status == UsaWrestlingVerificationStatus.Pending)
            return Results.BadRequest(new { message = "Status must be Current, Expired, or Rejected." });
        if (!Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var staffId)) return Results.Forbid();
        var verification = await db.UsaWrestlingVerifications.SingleOrDefaultAsync(x => x.Id == id);
        if (verification is null) return Results.NotFound();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var annualExpiration = new DateOnly(today.Year, 8, 31);
        if (annualExpiration < today) annualExpiration = annualExpiration.AddYears(1);
        verification.Status = status; verification.ExpiresOn = status == UsaWrestlingVerificationStatus.Current ? annualExpiration : request.ExpiresOn; verification.StaffNotes = request.StaffNotes?.Trim();
        verification.VerifiedOn = DateTime.UtcNow; verification.VerifiedByPortalUserId = staffId;
        await db.SaveChangesAsync(); return Results.Ok(ToDto(verification));
    }

    private static object ToDto(UsaWrestlingVerification x) => new { x.Id, x.AthleteId, x.MembershipNumber, status = x.Status.ToString(), x.SubmittedOn, x.VerifiedOn, x.ExpiresOn };
    [GeneratedRegex("^[A-Z0-9-]{3,50}$")] private static partial Regex MembershipNumberPattern();
}

public record SubmitUsaWrestlingRequest(string MembershipNumber);
public record ReviewUsaWrestlingRequest(string Status, DateOnly? ExpiresOn, string? StaffNotes);
