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
                        a.Gender
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
                        a.Gender
                    })
                })
                .FirstOrDefaultAsync();

            return family is null ? Results.NotFound() : Results.Ok(family);
        });

        return group;
    }
}
