using LegacyOS.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace LegacyOS.Api.Features.Dashboard;

public static class DashboardEndpoints
{
    public static void MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/staff/dashboard", async (LegacyOSDbContext db) => Results.Ok(new
        {
            families = await db.Families.CountAsync(),
            athletes = await db.Athletes.CountAsync(),
            activeEnrollments = await db.Enrollments.CountAsync(x => x.IsActive),
            pendingUsaWrestling = await db.UsaWrestlingVerifications.CountAsync(x =>
                x.Status == Features.UsaWrestling.UsaWrestlingVerificationStatus.Pending)
        })).RequireAuthorization("StaffOnly");
    }
}
