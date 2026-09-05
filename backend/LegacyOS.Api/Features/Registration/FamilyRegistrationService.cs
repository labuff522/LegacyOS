using LegacyOS.Api.Data;
using LegacyOS.Api.Features.Families;
using LegacyOS.Api.Features.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using LegacyOS.Api.Features.Activities;
using LegacyOS.Api.Features.Sessions;

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
        var organization = await _db.Organizations.Where(x => x.IsActive).OrderBy(x => x.CreatedOn).FirstOrDefaultAsync();

        if (organization is null)
        {
            throw new InvalidOperationException(
                "This installation has no internal organization record.");
        }

        var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == request.ProductId && x.IsActive && x.IsSessionPackage);

        if (product is null || product.ValidityDays is null)
        {
            throw new InvalidOperationException(
                "The selected session product is unavailable.");
        }

        var activeGroupIds = await _db.AthleteGroups.Where(x => x.IsActive).Select(x => x.Id).ToListAsync();
        if (request.Athletes.Any(x => !activeGroupIds.Contains(x.AthleteGroupId)))
            throw new InvalidOperationException("Choose an active Group for every athlete.");

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
            Gender = a.Gender,
            AthleteGroupId = a.AthleteGroupId
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

        var grantedOn = DateTime.UtcNow;
        var sessionLots = athletes.Select(athlete => new SessionCreditLot
        {
            Id = Guid.NewGuid(),
            AthleteId = athlete.Id,
            Athlete = athlete,
            ProductId = product.Id,
            Product = product,
            GrantSource = SessionGrantSource.PaidOutsideStripe,
            IsUnlimited = product.HasUnlimitedSessions,
            SessionsGranted = product.HasUnlimitedSessions ? null : product.SessionCount,
            SessionsRemaining = product.HasUnlimitedSessions ? null : product.SessionCount,
            GrantedOn = grantedOn,
            ExpiresOn = grantedOn.AddDays(product.ValidityDays.Value),
            IsActive = true,
        }).ToList();

        _db.SessionCreditLots.AddRange(sessionLots);
        _db.SessionLedgerEntries.AddRange(sessionLots.Select(lot => new SessionLedgerEntry
        {
            Id = Guid.NewGuid(), SessionCreditLot = lot, AthleteId = lot.AthleteId,
            EntryType = SessionLedgerEntryType.Grant,
            SessionChange = product.HasUnlimitedSessions ? 0 : product.SessionCount!.Value,
            Note = "Assigned during staff registration.", CreatedOn = grantedOn
        }));

        var activity = new Activity
{
        Id = Guid.NewGuid(),
        FamilyId = family.Id,
        Family = family,
        ActivityType = ActivityType.FamilyRegistered,
        Title = "Family registered",
        Description = $"{family.FamilyName} was registered in DenOS.",
        CreatedOn = DateTime.UtcNow
        };

        _db.Activities.Add(activity);

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
