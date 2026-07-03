using System.ComponentModel.DataAnnotations;

namespace LegacyOS.Api.Features.Families;

public class Family
{
    public Guid Id { get; set; }

    [MaxLength(150)]
    public string FamilyName { get; set; } = string.Empty;

    [MaxLength(150)]
    public string PrimaryContactName { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(25)]
    public string? Phone { get; set; }

    [MaxLength(500)]
    public string? BillingAddress { get; set; }

    [MaxLength(150)]
    public string? EmergencyContactName { get; set; }

    [MaxLength(25)]
    public string? EmergencyContactPhone { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public DateTime? ModifiedOn { get; set; }
}