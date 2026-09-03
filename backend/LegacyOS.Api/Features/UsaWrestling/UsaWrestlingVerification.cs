using LegacyOS.Api.Features.Families;
using LegacyOS.Api.Features.Portal;

namespace LegacyOS.Api.Features.UsaWrestling;

public class UsaWrestlingVerification
{
    public Guid Id { get; set; }
    public Guid AthleteId { get; set; }
    public Athlete Athlete { get; set; } = null!;
    public string MembershipNumber { get; set; } = string.Empty;
    public UsaWrestlingVerificationStatus Status { get; set; } = UsaWrestlingVerificationStatus.Pending;
    public DateTime SubmittedOn { get; set; } = DateTime.UtcNow;
    public DateTime? VerifiedOn { get; set; }
    public DateOnly? ExpiresOn { get; set; }
    public Guid? VerifiedByPortalUserId { get; set; }
    public PortalUser? VerifiedByPortalUser { get; set; }
    public string? StaffNotes { get; set; }
}

public enum UsaWrestlingVerificationStatus { Pending = 1, Current = 2, Expired = 3, Rejected = 4 }
