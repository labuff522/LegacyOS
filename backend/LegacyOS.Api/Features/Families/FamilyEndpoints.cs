using LegacyOS.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace LegacyOS.Api.Features.Families;

public static class FamilyEndpoints
{
    public static RouteGroupBuilder MapFamilyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/families");

        group.MapGet("/", async (LegacyOSDbContext db) =>
        {
            var families = await db.Families
                .Select(f => new FamilyResponse(
                    f.Id,
                    f.FamilyName,
                    f.PrimaryContactName,
                    f.Email,
                    f.Phone))
                .ToListAsync();

            return Results.Ok(families);
        });

        group.MapPost("/", async (
            CreateFamilyRequest request,
            LegacyOSDbContext db) =>
        {
            var family = new Family
            {
                Id = Guid.NewGuid(),
                FamilyName = request.FamilyName,
                PrimaryContactName = request.PrimaryContactName,
                Email = request.Email,
                Phone = request.Phone
            };

            db.Families.Add(family);

            await db.SaveChangesAsync();

            return Results.Created($"/families/{family.Id}", family);
        });

        return group;
    }
}