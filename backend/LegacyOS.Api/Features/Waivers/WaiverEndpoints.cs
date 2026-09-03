using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LegacyOS.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace LegacyOS.Api.Features.Waivers;

public static class WaiverEndpoints
{
    public static IEndpointRouteBuilder MapWaiverEndpoints(this IEndpointRouteBuilder app)
    {
        var staff = app.MapGroup("/staff/waivers").RequireAuthorization("StaffOnly");
        staff.MapGet("/", GlobalWaiversAsync);
        staff.MapPost("/", UploadGlobalAsync).DisableAntiforgery();
        staff.MapGet("/families/{familyId:guid}", StaffFamilyWaiversAsync);
        staff.MapPost("/families/{familyId:guid}", UploadAsync).DisableAntiforgery();
        staff.MapGet("/{id:guid}/file", FileAsync);
        staff.MapPut("/{id:guid}/status", StatusAsync);

        var portal = app.MapGroup("/portal/waivers").RequireAuthorization("CustomerOnly");
        portal.MapGet("/", PortalWaiversAsync);
        portal.MapGet("/{id:guid}/file", PortalFileAsync);
        portal.MapPost("/{id:guid}/sign", SignAsync);
        app.MapGet("/portal/auth/waivers/{id:guid}/file", PublicRegistrationFileAsync).AllowAnonymous();
        return app;
    }

    private static async Task<IResult> GlobalWaiversAsync(LegacyOSDbContext db) => Results.Ok(await db.WaiverTemplates
        .OrderByDescending(x => x.CreatedOn).Select(x => new { x.Id, x.Name, x.Version, x.FileName, x.IsRequired,
            x.IsActive, x.CreatedOn, signatureCount = db.WaiverSignatures.Count(s => s.WaiverTemplateId == x.Id) }).ToListAsync());

    private static async Task<IResult> UploadGlobalAsync(HttpRequest request, LegacyOSDbContext db)
    {
        var organizationId = await db.Organizations.Where(x => x.IsActive).OrderBy(x => x.CreatedOn).Select(x => (Guid?)x.Id).FirstOrDefaultAsync();
        if (organizationId is null) return Results.Problem("This installation has no internal organization record.");
        return await UploadCoreAsync(organizationId.Value, request, db);
    }

    private static async Task<IResult> StaffFamilyWaiversAsync(Guid familyId, LegacyOSDbContext db)
    {
        if (!await db.Families.AnyAsync(x => x.Id == familyId)) return Results.NotFound();
        var templates = await db.WaiverTemplates.OrderByDescending(x => x.CreatedOn)
            .Select(x => new { x.Id, x.Name, x.Version, x.FileName, x.IsRequired, x.IsActive, x.CreatedOn,
                signatures = db.WaiverSignatures.Where(s => s.WaiverTemplateId == x.Id && s.FamilyId == familyId)
                    .Select(s => new { s.Id, s.AthleteId, athleteName = s.Athlete.FirstName + " " + s.Athlete.LastName, s.SignedName, s.SignedOn, s.ExpiresOn }).ToList() }).ToListAsync();
        return Results.Ok(templates);
    }

    private static async Task<IResult> UploadAsync(Guid familyId, HttpRequest request, LegacyOSDbContext db)
    {
        if (!request.HasFormContentType) return Results.BadRequest(new { message = "Upload a PDF waiver." });
        var form = await request.ReadFormAsync(); var file = form.Files.GetFile("file");
        if (file is null || file.Length == 0 || file.Length > 10 * 1024 * 1024 || file.ContentType != "application/pdf")
            return Results.BadRequest(new { message = "A PDF of 10 MB or less is required." });
        if (!Guid.TryParse(form["organizationId"], out var organizationId) ||
            !await db.FamilyOrganizations.AnyAsync(x => x.FamilyId == familyId && x.OrganizationId == organizationId && x.IsActive))
            return Results.BadRequest(new { message = "Choose an organization associated with this family." });
        return await UploadCoreAsync(organizationId, request, db, form);
    }

    private static async Task<IResult> UploadCoreAsync(Guid organizationId, HttpRequest request, LegacyOSDbContext db, IFormCollection? existingForm = null)
    {
        if (!request.HasFormContentType) return Results.BadRequest(new { message = "Upload a PDF waiver." });
        var form = existingForm ?? await request.ReadFormAsync(); var file = form.Files.GetFile("file");
        if (file is null || file.Length == 0 || file.Length > 10 * 1024 * 1024 || file.ContentType != "application/pdf")
            return Results.BadRequest(new { message = "A PDF of 10 MB or less is required." });
        var name = form["name"].ToString().Trim(); if (name.Length == 0) return Results.BadRequest(new { message = "Waiver name is required." });
        await using var stream = new MemoryStream(); await file.CopyToAsync(stream); var bytes = stream.ToArray();
        if (bytes.Length < 5 || Encoding.ASCII.GetString(bytes, 0, 5) != "%PDF-")
            return Results.BadRequest(new { message = "The uploaded file is not a valid PDF." });
        var prior = await db.WaiverTemplates.Where(x => x.OrganizationId == organizationId && x.Name == name).ToListAsync();
        foreach (var item in prior) item.IsActive = false;
        var waiver = new WaiverTemplate { Id = Guid.NewGuid(), OrganizationId = organizationId, Name = name,
            Version = prior.Count == 0 ? 1 : prior.Max(x => x.Version) + 1, FileName = Path.GetFileName(file.FileName),
            ContentType = "application/pdf", FileContent = bytes, Sha256 = Convert.ToHexStringLower(SHA256.HashData(bytes)),
            IsRequired = !bool.TryParse(form["isRequired"], out var required) || required, IsActive = true, CreatedOn = DateTime.UtcNow };
        db.WaiverTemplates.Add(waiver); await db.SaveChangesAsync(); return Results.Created($"/staff/waivers/{waiver.Id}", new { waiver.Id });
    }

    private static async Task<IResult> FileAsync(Guid id, LegacyOSDbContext db)
    {
        var waiver = await db.WaiverTemplates.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        return waiver is null ? Results.NotFound() : Results.File(waiver.FileContent, waiver.ContentType, waiver.FileName);
    }

    private static async Task<IResult> StatusAsync(Guid id, WaiverStatusRequest request, LegacyOSDbContext db)
    {
        var waiver = await db.WaiverTemplates.SingleOrDefaultAsync(x => x.Id == id); if (waiver is null) return Results.NotFound();
        waiver.IsActive = request.IsActive; waiver.IsRequired = request.IsRequired; await db.SaveChangesAsync(); return Results.NoContent();
    }

    private static async Task<IResult> PortalWaiversAsync(ClaimsPrincipal principal, LegacyOSDbContext db)
    {
        var guardianId = GuardianId(principal); if (guardianId is null) return Results.Forbid();
        var family = await db.Guardians.Where(x => x.Id == guardianId).Select(x => new { x.FamilyId,
            athletes = x.Family.Athletes.Select(a => new { a.Id, a.FirstName, a.LastName }).ToList() }).SingleOrDefaultAsync();
        if (family is null) return Results.NotFound();
        var waivers = await db.WaiverTemplates.Where(x => x.IsActive).Select(x => new
        { x.Id, x.Name, x.Version, x.FileName, x.IsRequired,
          signedAthleteIds = db.WaiverSignatures.Where(s => s.WaiverTemplateId == x.Id && s.FamilyId == family.FamilyId && s.ExpiresOn > DateTime.UtcNow).Select(s => s.AthleteId).Distinct().ToList() }).ToListAsync();
        return Results.Ok(new { family.athletes, waivers });
    }

    private static async Task<IResult> PortalFileAsync(Guid id, ClaimsPrincipal principal, LegacyOSDbContext db)
    {
        var guardianId = GuardianId(principal); if (guardianId is null) return Results.Forbid();
        var waiver = await db.WaiverTemplates.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && x.IsActive);
        return waiver is null ? Results.NotFound() : Results.File(waiver.FileContent, waiver.ContentType, waiver.FileName);
    }

    private static async Task<IResult> SignAsync(Guid id, SignWaiverRequest request, ClaimsPrincipal principal, HttpRequest http, LegacyOSDbContext db)
    {
        var guardianId = GuardianId(principal);
        if (guardianId is null || !Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ||
            !request.Accepted || string.IsNullOrWhiteSpace(request.SignedName)) return Results.BadRequest(new { message = "Consent and a typed legal name are required." });
        var guardian = await db.Guardians.SingleOrDefaultAsync(x => x.Id == guardianId);
        if (guardian is null || !await db.Athletes.AnyAsync(x => x.Id == request.AthleteId && x.FamilyId == guardian.FamilyId)) return Results.Forbid();
        var waiver = await db.WaiverTemplates.SingleOrDefaultAsync(x => x.Id == id && x.IsActive);
        if (waiver is null) return Results.NotFound();
        if (await db.WaiverSignatures.AnyAsync(x => x.WaiverTemplateId == id && x.AthleteId == request.AthleteId && x.ExpiresOn > DateTime.UtcNow)) return Results.Conflict(new { message = "This waiver version is already current for the athlete." });
        var signedOn = DateTime.UtcNow;
        var signature = new WaiverSignature { Id = Guid.NewGuid(), WaiverTemplateId = waiver.Id, FamilyId = guardian.FamilyId,
            AthleteId = request.AthleteId, GuardianId = guardian.Id, PortalUserId = userId, SignedName = request.SignedName.Trim(),
            WaiverSha256 = waiver.Sha256, IpAddress = http.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            UserAgent = http.Headers.UserAgent.ToString(), SignedOn = signedOn, ExpiresOn = signedOn.AddDays(365) };
        db.WaiverSignatures.Add(signature); await db.SaveChangesAsync(); return Results.Created($"/portal/waivers/signatures/{signature.Id}", new { signature.Id, signature.SignedOn, signature.ExpiresOn });
    }

    private static Guid? GuardianId(ClaimsPrincipal principal) => Guid.TryParse(principal.FindFirstValue("guardian_id"), out var id) ? id : null;

    private static async Task<IResult> PublicRegistrationFileAsync(Guid id, LegacyOSDbContext db)
    {
        var waiver = await db.WaiverTemplates.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && x.IsActive);
        return waiver is null ? Results.NotFound() : Results.File(waiver.FileContent, waiver.ContentType, waiver.FileName);
    }
}

public record WaiverStatusRequest(bool IsActive, bool IsRequired);
public record SignWaiverRequest(Guid AthleteId, string SignedName, bool Accepted);
