using System.ComponentModel.DataAnnotations;

namespace LegacyOS.Api.Features.Families;

public class AthleteGroup
{
    public Guid Id { get; set; }
    [MaxLength(100)] public string Name { get; set; } = string.Empty;
    [MaxLength(1000)] public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public ICollection<Athlete> Athletes { get; set; } = new List<Athlete>();
}
