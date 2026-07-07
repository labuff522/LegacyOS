using LegacyOS.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace LegacyOS.Api.Features.Activities;

public static class ActivityEndpoints
{
    public static RouteGroupBuilder MapActivityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/activities");

        group.MapGet("/family/{familyId:guid}", async (Guid familyId, LegacyOSDbContext db) =>
        {
            var activities = await db.Activities
                .Where(a => a.FamilyId == familyId)
                .OrderByDescending(a => a.CreatedOn)
                .Select(a => new
                {
                    a.Id,
                    ActivityType = a.ActivityType.ToString(),
                    a.Title,
                    a.Description,
                    a.CreatedOn
                })
                .ToListAsync();

            return Results.Ok(activities);
        });

        return group;
    }
}