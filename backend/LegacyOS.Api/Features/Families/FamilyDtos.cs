namespace LegacyOS.Api.Features.Families;

public record CreateFamilyRequest(
    string FamilyName,
    string PrimaryContactName,
    string Email,
    string? Phone);

public record FamilyResponse(
    Guid Id,
    string FamilyName,
    string PrimaryContactName,
    string Email,
    string? Phone);