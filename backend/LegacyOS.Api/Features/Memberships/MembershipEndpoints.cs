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

        group.MapGet("/admin-catalog", async (LegacyOSDbContext db) => Results.Ok(new
        {
            organizations = await db.Organizations.Where(x => x.IsActive).OrderBy(x => x.Name).Select(x => new { x.Id, x.Name }).ToListAsync(),
            services = await db.Services.OrderBy(x => x.Name).Select(x => new { x.Id, x.Name, x.ShortName, x.OrganizationId, organizationName = x.Organization.Name, x.IsActive }).ToListAsync()
        }));

        group.MapPost("/services", async (ServiceRequest request, LegacyOSDbContext db) =>
        {
            if (!await db.Organizations.AnyAsync(x => x.Id == request.OrganizationId && x.IsActive)) return Results.BadRequest();
            var service = new Service { Id = Guid.NewGuid(), Name = request.Name.Trim(), ShortName = request.ShortName.Trim(),
                OrganizationId = request.OrganizationId, IsActive = true, CreatedOn = DateTime.UtcNow };
            db.Services.Add(service); await db.SaveChangesAsync(); return Results.Created($"/membership-plans/services/{service.Id}", new { service.Id });
        });

        group.MapPut("/services/{id:guid}", async (Guid id, UpdateServiceRequest request, LegacyOSDbContext db) =>
        {
            var service = await db.Services.SingleOrDefaultAsync(x => x.Id == id); if (service is null) return Results.NotFound();
            service.Name = request.Name.Trim(); service.ShortName = request.ShortName.Trim(); service.OrganizationId = request.OrganizationId; service.IsActive = request.IsActive;
            await db.SaveChangesAsync(); return Results.NoContent();
        });

        group.MapPost("/", async (MembershipPlanRequest request, LegacyOSDbContext db) =>
        {
            if (!await db.Organizations.AnyAsync(x => x.Id == request.OrganizationId && x.IsActive)) return Results.BadRequest();
            var plan = new MembershipPlan { Id = Guid.NewGuid(), Name = request.Name.Trim(), ShortName = request.ShortName.Trim(),
                OrganizationId = request.OrganizationId, MonthlyPrice = request.MonthlyPrice, IsActive = true, CreatedOn = DateTime.UtcNow };
            plan.PlanServices = request.ServiceIds.Distinct().Select(id => new PlanService { MembershipPlan = plan, ServiceId = id }).ToList();
            db.MembershipPlans.Add(plan); await db.SaveChangesAsync(); return Results.Created($"/membership-plans/{plan.Id}", new { plan.Id });
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateMembershipPlanRequest request, LegacyOSDbContext db) =>
        {
            var plan = await db.MembershipPlans.Include(x => x.PlanServices).SingleOrDefaultAsync(x => x.Id == id); if (plan is null) return Results.NotFound();
            plan.Name = request.Name.Trim(); plan.ShortName = request.ShortName.Trim(); plan.OrganizationId = request.OrganizationId;
            plan.MonthlyPrice = request.MonthlyPrice; plan.IsActive = request.IsActive;
            db.PlanServices.RemoveRange(plan.PlanServices); plan.PlanServices = request.ServiceIds.Distinct().Select(serviceId => new PlanService { MembershipPlanId = plan.Id, ServiceId = serviceId }).ToList();
            await db.SaveChangesAsync(); return Results.NoContent();
        });

        return group;
    }
}

public record ServiceRequest(Guid OrganizationId, string Name, string ShortName);
public record UpdateServiceRequest(Guid OrganizationId, string Name, string ShortName, bool IsActive);
public record MembershipPlanRequest(Guid OrganizationId, string Name, string ShortName, decimal MonthlyPrice, List<Guid> ServiceIds);
public record UpdateMembershipPlanRequest(Guid OrganizationId, string Name, string ShortName, decimal MonthlyPrice, List<Guid> ServiceIds, bool IsActive);
