using LegacyOS.Api.Features.Families;

namespace LegacyOS.Api.Features.Organizations;

public class FamilyOrganization
{
    public Guid FamilyId { get; set; }

    public Family Family { get; set; } = null!;

    public Guid OrganizationId { get; set; }

    public Organization Organization { get; set; } = null!;

    public DateTime JoinedOn { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;
}