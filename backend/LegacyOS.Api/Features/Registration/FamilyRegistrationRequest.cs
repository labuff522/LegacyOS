namespace LegacyOS.Api.Features.Registration;

public class FamilyRegistrationRequest
{
    public string FamilyName { get; set; } = "";

    public GuardianRequest Guardian { get; set; } = new();

    public List<AthleteRequest> Athletes { get; set; } = new();
}

public class GuardianRequest
{
    public string FirstName { get; set; } = "";

    public string LastName { get; set; } = "";

    public string Email { get; set; } = "";

    public string Phone { get; set; } = "";
}

public class AthleteRequest
{
    public string FirstName { get; set; } = "";

    public string LastName { get; set; } = "";

    public DateOnly DateOfBirth { get; set; }

    public string? Gender { get; set; }
}