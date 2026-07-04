using System.ComponentModel.DataAnnotations;

namespace LegacyOS.Api.Features.Families;

public class Athlete
{
    public Guid Id { get; set; }

    public Guid FamilyId { get; set; }

    public Family Family { get; set; } = null!;

    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    public DateOnly DateOfBirth { get; set; }

    [MaxLength(25)]
    public string? Gender { get; set; }
}