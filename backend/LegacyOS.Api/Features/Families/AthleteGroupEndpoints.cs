using LegacyOS.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace LegacyOS.Api.Features.Families;

public static class AthleteGroupEndpoints
{
    public static IEndpointRouteBuilder MapAthleteGroupEndpoints(this IEndpointRouteBuilder app)
    {
        var staff = app.MapGroup("/athlete-groups").RequireAuthorization("StaffOnly");
        staff.MapGet("/", async (LegacyOSDbContext db) => Results.Ok(await db.AthleteGroups.OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name, x.Description, x.IsActive, athleteCount = x.Athletes.Count }).ToListAsync()));
        staff.MapPost("/", async (AthleteGroupRequest request, LegacyOSDbContext db) =>
        {
            var validation = Validate(request); if (validation is not null) return validation;
            var name = request.Name.Trim();
            if (await db.AthleteGroups.AnyAsync(x => x.Name.ToLower() == name.ToLower())) return Results.Conflict(new { message = "A group with this name already exists." });
            var item = new AthleteGroup { Id = Guid.NewGuid(), Name = name, Description = request.Description.Trim(), IsActive = true };
            db.AthleteGroups.Add(item); await db.SaveChangesAsync(); return Results.Created($"/athlete-groups/{item.Id}", item);
        });
        staff.MapPut("/{id:guid}", async (Guid id, AthleteGroupRequest request, LegacyOSDbContext db) =>
        {
            var validation = Validate(request); if (validation is not null) return validation;
            var item = await db.AthleteGroups.SingleOrDefaultAsync(x => x.Id == id); if (item is null) return Results.NotFound();
            var name = request.Name.Trim();
            if (await db.AthleteGroups.AnyAsync(x => x.Id != id && x.Name.ToLower() == name.ToLower())) return Results.Conflict(new { message = "A group with this name already exists." });
            item.Name = name; item.Description = request.Description.Trim(); item.IsActive = request.IsActive;
            await db.SaveChangesAsync(); return Results.NoContent();
        });
        return app;
    }

    private static IResult? Validate(AthleteGroupRequest request) =>
        string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Description)
            ? Results.ValidationProblem(new Dictionary<string, string[]> { ["group"] = ["Name and description are required."] }) : null;
}

public record AthleteGroupRequest(string Name, string Description, bool IsActive = true);
