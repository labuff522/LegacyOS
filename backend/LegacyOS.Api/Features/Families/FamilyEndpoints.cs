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
                .Select(f => new
                {
                    f.Id,
                    f.FamilyName,
                    f.IsActive,
                    f.CreatedOn
                })
                .ToListAsync();

            return Results.Ok(families);
        });

        return group;
    }
}