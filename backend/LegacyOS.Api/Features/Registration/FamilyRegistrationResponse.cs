namespace LegacyOS.Api.Features.Registration;

public class FamilyRegistrationResponse
{
    public Guid FamilyId { get; set; }

    public string FamilyName { get; set; } = "";

    public Guid GuardianId { get; set; }

    public List<Guid> AthleteIds { get; set; } = new();
}