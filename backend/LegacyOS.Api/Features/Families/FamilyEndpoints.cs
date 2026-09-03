using LegacyOS.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace LegacyOS.Api.Features.Families;

public static class FamilyEndpoints
{
    public static RouteGroupBuilder MapFamilyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/families").RequireAuthorization("StaffOnly");

        group.MapGet("/", async (LegacyOSDbContext db) =>
        {
            var families = await db.Families
                .Include(f => f.Guardians)
                .Include(f => f.Athletes)
                .Include(f => f.FamilyOrganizations)
                    .ThenInclude(fo => fo.Organization)
                .Select(f => new
                {
                    f.Id,
                    f.FamilyName,
                    f.IsActive,
                    Organizations = f.FamilyOrganizations.Select(fo => new
                    {
                        fo.Organization.Id,
                        fo.Organization.Name,
                        fo.Organization.ShortName,
                        fo.IsActive,
                        fo.JoinedOn
                    }),
                    Guardians = f.Guardians.Select(g => new
                    {
                        g.Id,
                        g.FirstName,
                        g.LastName,
                        g.Email,
                        g.Phone,
                        g.IsPrimaryContact
                    }),
                    Athletes = f.Athletes.Select(a => new
                    {
                        a.Id,
                        a.FirstName,
                        a.LastName,
                        a.DateOfBirth,
                        a.Gender,
                        SessionPackages = db.SessionCreditLots.Where(l => l.AthleteId == a.Id && l.IsActive)
                            .OrderBy(l => l.ExpiresOn).Select(l => new { l.Id, productName = l.Product.Name,
                                l.IsUnlimited, l.SessionsRemaining, l.ExpiresOn }).ToList(),
                        MissingRequiredWaivers = db.WaiverTemplates.Count(w => w.IsActive && w.IsRequired &&
                            !db.WaiverSignatures.Any(s => s.WaiverTemplateId == w.Id && s.AthleteId == a.Id && s.ExpiresOn > DateTime.UtcNow))
                    })
                })
                .ToListAsync();

            return Results.Ok(families);
        });

        group.MapGet("/{id:guid}", async (Guid id, LegacyOSDbContext db) =>
        {
            var family = await db.Families
                .Include(f => f.Guardians)
                .Include(f => f.Athletes)
                .Include(f => f.FamilyOrganizations)
                    .ThenInclude(fo => fo.Organization)
                .Where(f => f.Id == id)
                .Select(f => new
                {
                    f.Id,
                    f.FamilyName,
                    f.IsActive,
                    Organizations = f.FamilyOrganizations.Select(fo => new
                    {
                        fo.Organization.Id,
                        fo.Organization.Name,
                        fo.Organization.ShortName,
                        fo.IsActive,
                        fo.JoinedOn
                    }),
                    Guardians = f.Guardians.Select(g => new
                    {
                        g.Id,
                        g.FirstName,
                        g.LastName,
                        g.Email,
                        g.Phone,
                        g.IsPrimaryContact
                    }),
                    Athletes = f.Athletes.Select(a => new
                    {
                        a.Id,
                        a.FirstName,
                        a.LastName,
                        a.DateOfBirth,
                        a.Gender,
                        SessionPackages = db.SessionCreditLots.Where(l => l.AthleteId == a.Id && l.IsActive)
                            .OrderBy(l => l.ExpiresOn).Select(l => new { l.Id, productName = l.Product.Name,
                                l.IsUnlimited, l.SessionsRemaining, l.ExpiresOn }).ToList(),
                        MissingRequiredWaivers = db.WaiverTemplates.Count(w => w.IsActive && w.IsRequired &&
                            !db.WaiverSignatures.Any(s => s.WaiverTemplateId == w.Id && s.AthleteId == a.Id && s.ExpiresOn > DateTime.UtcNow))
                    })
                })
                .FirstOrDefaultAsync();

            return family is null ? Results.NotFound() : Results.Ok(family);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateFamilyRequest request, LegacyOSDbContext db) =>
        {
            var family = await db.Families.SingleOrDefaultAsync(x => x.Id == id); if (family is null) return Results.NotFound();
            family.FamilyName = request.FamilyName.Trim(); family.IsActive = request.IsActive; family.ModifiedOn = DateTime.UtcNow;
            await db.SaveChangesAsync(); return Results.NoContent();
        });

        group.MapGet("/{id:guid}/orders", async (Guid id, LegacyOSDbContext db) => Results.Ok(await db.PurchaseOrders
            .Where(x => x.FamilyId == id).OrderByDescending(x => x.CreatedOn).Select(x => new { x.Id, kind = x.Kind.ToString(),
                status = x.Status.ToString(), x.OriginalAmount, x.DiscountAmount, x.Amount, x.Currency, x.DiscountCodeSnapshot,
                x.FamilySnapshotJson, x.AthleteSnapshotJson, x.ItemSnapshotJson, x.CreatedOn, x.CompletedOn }).ToListAsync()));

        group.MapPut("/{familyId:guid}/guardians/{guardianId:guid}", async (Guid familyId, Guid guardianId, UpdateGuardianRequest request, LegacyOSDbContext db) =>
        {
            var guardian = await db.Guardians.SingleOrDefaultAsync(x => x.Id == guardianId && x.FamilyId == familyId); if (guardian is null) return Results.NotFound();
            guardian.FirstName = request.FirstName.Trim(); guardian.LastName = request.LastName.Trim(); guardian.Email = request.Email.Trim(); guardian.Phone = request.Phone.Trim(); guardian.IsPrimaryContact = request.IsPrimaryContact;
            await db.SaveChangesAsync(); return Results.NoContent();
        });

        group.MapPost("/{familyId:guid}/athletes", async (Guid familyId, AthleteEditRequest request, LegacyOSDbContext db) =>
        {
            if (!await db.Families.AnyAsync(x => x.Id == familyId)) return Results.NotFound();
            var athlete = new Athlete { Id = Guid.NewGuid(), FamilyId = familyId, FirstName = request.FirstName.Trim(), LastName = request.LastName.Trim(), DateOfBirth = request.DateOfBirth, Gender = request.Gender?.Trim() };
            db.Athletes.Add(athlete); await db.SaveChangesAsync(); return Results.Created($"/families/{familyId}/athletes/{athlete.Id}", new { athlete.Id });
        });

        group.MapPut("/{familyId:guid}/athletes/{athleteId:guid}", async (Guid familyId, Guid athleteId, AthleteEditRequest request, LegacyOSDbContext db) =>
        {
            var athlete = await db.Athletes.SingleOrDefaultAsync(x => x.Id == athleteId && x.FamilyId == familyId); if (athlete is null) return Results.NotFound();
            athlete.FirstName = request.FirstName.Trim(); athlete.LastName = request.LastName.Trim(); athlete.DateOfBirth = request.DateOfBirth; athlete.Gender = request.Gender?.Trim();
            await db.SaveChangesAsync(); return Results.NoContent();
        });

        return group;
    }
}

public record UpdateFamilyRequest(string FamilyName, bool IsActive);
public record UpdateGuardianRequest(string FirstName, string LastName, string Email, string Phone, bool IsPrimaryContact);
public record AthleteEditRequest(string FirstName, string LastName, DateOnly DateOfBirth, string? Gender);
