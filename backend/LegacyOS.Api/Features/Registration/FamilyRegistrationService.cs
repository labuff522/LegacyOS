using LegacyOS.Api.Data;
using LegacyOS.Api.Features.Families;
using LegacyOS.Api.Features.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace LegacyOS.Api.Features.Registration;

public class FamilyRegistrationService
{
    private readonly LegacyOSDbContext _db;

    public FamilyRegistrationService(LegacyOSDbContext db)
    {
        _db = db;
    }

    public async Task<FamilyRegistrationResponse> RegisterFamilyAsync(
        FamilyRegistrationRequest request)
    {
        var organization = await _db.Organizations
            .FirstOrDefaultAsync(x => x.ShortName == request.OrganizationShortName);

        if (organization is null)
        {
            throw new InvalidOperationException(
                $"Organization '{request.OrganizationShortName}' was not found.");
        }

        await using IDbContextTransaction transaction =
            await _db.Database.BeginTransactionAsync();

        var family = new Family
        {
            Id = Guid.NewGuid(),
            FamilyName = request.FamilyName,
            IsActive = true,
            CreatedOn = DateTime.UtcNow
        };

        var guardian = new Guardian
        {
            Id = Guid.NewGuid(),
            FamilyId = family.Id,
            Family = family,
            FirstName = request.Guardian.FirstName,
            LastName = request.Guardian.LastName,
            Email = request.Guardian.Email,
            Phone = request.Guardian.Phone,
            IsPrimaryContact = true,
            ReceivesBilling = true,
            ReceivesSms = true
        };

        var athletes = request.Athletes.Select(a => new Athlete
        {
            Id = Guid.NewGuid(),
            FamilyId = family.Id,
            Family = family,
            FirstName = a.FirstName,
            LastName = a.LastName,
            DateOfBirth = a.DateOfBirth,
            Gender = a.Gender
        }).ToList();

        var familyOrganization = new FamilyOrganization
        {
            FamilyId = family.Id,
            Family = family,
            OrganizationId = organization.Id,
            Organization = organization,
            JoinedOn = DateTime.UtcNow,
            IsActive = true
        };

        _db.Families.Add(family);
        _db.Guardians.Add(guardian);
        _db.Athletes.AddRange(athletes);
        _db.FamilyOrganizations.Add(familyOrganization);

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return new FamilyRegistrationResponse
        {
            FamilyId = family.Id,
            FamilyName = family.FamilyName,
            GuardianId = guardian.Id,
            AthleteIds = athletes.Select(a => a.Id).ToList()
        };
    }
}