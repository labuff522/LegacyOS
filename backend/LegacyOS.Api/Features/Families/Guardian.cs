using System.ComponentModel.DataAnnotations;

namespace LegacyOS.Api.Features.Families;

public class Guardian
{
    public Guid Id { get; set; }

    public Guid FamilyId { get; set; }

    public Family Family { get; set; } = null!;

    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(25)]
    public string Phone { get; set; } = string.Empty;

    public bool IsPrimaryContact { get; set; }

    public bool ReceivesBilling { get; set; }

    public bool ReceivesSms { get; set; }
}