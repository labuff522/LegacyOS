using LegacyOS.Api.Features.Families;
using LegacyOS.Api.Features.Memberships;

namespace LegacyOS.Api.Features.Enrollments;

public class Enrollment
{
    public Guid Id { get; set; }

    public Guid AthleteId { get; set; }

    public Athlete Athlete { get; set; } = null!;

    public Guid MembershipPlanId { get; set; }

    public MembershipPlan MembershipPlan { get; set; } = null!;

    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}