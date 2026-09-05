using System.ComponentModel.DataAnnotations;
using LegacyOS.Api.Features.Families;

namespace LegacyOS.Api.Features.Portal;

public class PortalUser
{
    public Guid Id { get; set; }
    [MaxLength(255)] public string Email { get; set; } = string.Empty;
    [MaxLength(255)] public string NormalizedEmail { get; set; } = string.Empty;
    [MaxLength(500)] public string PasswordHash { get; set; } = string.Empty;
    [MaxLength(25)] public string Role { get; set; } = PortalRoles.Customer;
    public Guid? GuardianId { get; set; }
    public Guardian? Guardian { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}

public class PortalAccessToken
{
    public Guid Id { get; set; }
    public Guid PortalUserId { get; set; }
    public PortalUser PortalUser { get; set; } = null!;
    [MaxLength(64)] public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresOn { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedOn { get; set; }
}

public class GuardianInvitation
{
    public Guid Id { get; set; }
    public Guid GuardianId { get; set; }
    public Guardian Guardian { get; set; } = null!;
    [MaxLength(64)] public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresOn { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public DateTime? AcceptedOn { get; set; }
}

public class PortalPasswordResetToken
{
    public Guid Id { get; set; }
    public Guid PortalUserId { get; set; }
    public PortalUser PortalUser { get; set; } = null!;
    [MaxLength(64)] public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresOn { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public DateTime? UsedOn { get; set; }
}

public static class PortalRoles
{
    public const string Customer = "Customer";
    public const string Staff = "Staff";
}
