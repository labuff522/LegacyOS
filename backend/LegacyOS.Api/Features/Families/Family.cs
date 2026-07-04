using System.ComponentModel.DataAnnotations;

namespace LegacyOS.Api.Features.Families;

public class Family
{
    public Guid Id { get; set; }

    [MaxLength(150)]
    public string FamilyName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public DateTime? ModifiedOn { get; set; }

    public ICollection<Guardian> Guardians { get; set; } = new List<Guardian>();

    public ICollection<Athlete> Athletes { get; set; } = new List<Athlete>();
}