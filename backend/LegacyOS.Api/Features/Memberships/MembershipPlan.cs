using LegacyOS.Api.Features.Organizations;

namespace LegacyOS.Api.Features.Memberships;

public class MembershipPlan
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public Organization Organization { get; set; } = null!;

    public string Name { get; set; } = "";

    public string ShortName { get; set; } = "";

    public decimal MonthlyPrice { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public ICollection<PlanService> PlanServices { get; set; } = new List<PlanService>();
}