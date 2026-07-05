namespace LegacyOS.Api.Features.Memberships;

public class PlanService
{
    public Guid MembershipPlanId { get; set; }

    public MembershipPlan MembershipPlan { get; set; } = null!;

    public Guid ServiceId { get; set; }

    public Service Service { get; set; } = null!;
}