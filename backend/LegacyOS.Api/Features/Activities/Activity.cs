using LegacyOS.Api.Features.Families;

namespace LegacyOS.Api.Features.Activities;

public class Activity
{
    public Guid Id { get; set; }

    public Guid FamilyId { get; set; }

    public Family Family { get; set; } = null!;

    public ActivityType ActivityType { get; set; } = ActivityType.Other;

    public string Title { get; set; } = "";

    public string? Description { get; set; }

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}