using LegacyOS.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace LegacyOS.Api.Features.Memberships;

public static class MembershipEndpoints
{
    public static RouteGroupBuilder MapMembershipEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/membership-plans").RequireAuthorization("StaffOnly");

        group.MapGet("/", async (LegacyOSDbContext db) =>
        {
            var plans = await db.MembershipPlans
                .Include(p => p.Organization)
                .Include(p => p.PlanServices)
                    .ThenInclude(ps => ps.Service)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.ShortName,
                    p.MonthlyPrice,
                    Organization = new
                    {
                        p.Organization.Id,
                        p.Organization.Name,
                        p.Organization.ShortName
                    },
                    Services = p.PlanServices.Select(ps => new
                    {
                        ps.Service.Id,
                        ps.Service.Name,
                        ps.Service.ShortName
                    })
                })
                .ToListAsync();

            return Results.Ok(plans);
        });

        return group;
    }
}
