namespace LegacyOS.Api.Features.Organizations;

public class Organization
{
    public Guid Id { get; set; }

    public string Name { get; set; } = "";

    public string ShortName { get; set; } = "";

    public OrganizationType OrganizationType { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public ICollection<FamilyOrganization> FamilyOrganizations { get; set; }
        = new List<FamilyOrganization>();
}