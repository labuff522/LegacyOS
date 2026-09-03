using LegacyOS.Api.Features.Families;
using LegacyOS.Api.Features.Organizations;
using LegacyOS.Api.Features.Portal;

namespace LegacyOS.Api.Features.Waivers;

public class WaiverTemplate
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public string Name { get; set; } = "";
    public int Version { get; set; }
    public string FileName { get; set; } = "";
    public string ContentType { get; set; } = "application/pdf";
    public byte[] FileContent { get; set; } = [];
    public string Sha256 { get; set; } = "";
    public bool IsRequired { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}

public class WaiverSignature
{
    public Guid Id { get; set; }
    public Guid WaiverTemplateId { get; set; }
    public WaiverTemplate WaiverTemplate { get; set; } = null!;
    public Guid FamilyId { get; set; }
    public Family Family { get; set; } = null!;
    public Guid AthleteId { get; set; }
    public Athlete Athlete { get; set; } = null!;
    public Guid GuardianId { get; set; }
    public Guardian Guardian { get; set; } = null!;
    public Guid PortalUserId { get; set; }
    public PortalUser PortalUser { get; set; } = null!;
    public string SignedName { get; set; } = "";
    public string WaiverSha256 { get; set; } = "";
    public string IpAddress { get; set; } = "";
    public string UserAgent { get; set; } = "";
    public DateTime SignedOn { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresOn { get; set; }
}
