using LegacyOS.Api.Data;
using LegacyOS.Api.Features.Memberships;
using LegacyOS.Api.Features.Organizations;
using Microsoft.EntityFrameworkCore;
using LegacyOS.Api.Features.Portal;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;

namespace LegacyOS.Api.Infrastructure;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<LegacyOSDbContext>();

        await db.Database.MigrateAsync();

        await BackfillOrderSnapshotsAsync(db);

        await EnsureBootstrapStaffAsync(scope.ServiceProvider, db);

        if (!await db.Organizations.AnyAsync())
        {
            var wolfpack = new Organization
            {
                Id = Guid.NewGuid(),
                Name = "Wolfpack Wrestling Club",
                ShortName = "Wolfpack",
                OrganizationType = OrganizationType.NonProfit,
                IsActive = true,
                CreatedOn = DateTime.UtcNow
            };

            var theDen = new Organization
            {
                Id = Guid.NewGuid(),
                Name = "The Den Franklin",
                ShortName = "TheDen",
                OrganizationType = OrganizationType.Commercial,
                IsActive = true,
                CreatedOn = DateTime.UtcNow
            };

            db.Organizations.AddRange(wolfpack, theDen);

            var openMat = new Service
            {
                Id = Guid.NewGuid(),
                Organization = wolfpack,
                Name = "Open Mat",
                ShortName = "OpenMat"
            };

            var seriousYouth = new Service
            {
                Id = Guid.NewGuid(),
                Organization = wolfpack,
                Name = "Serious Youth Wrestlers",
                ShortName = "SeriousYouth"
            };

            var hsWrestling = new Service
            {
                Id = Guid.NewGuid(),
                Organization = wolfpack,
                Name = "HS Wrestling",
                ShortName = "HSWrestling"
            };

            var onlineTraining = new Service
            {
                Id = Guid.NewGuid(),
                Organization = wolfpack,
                Name = "Online Training",
                ShortName = "OnlineTraining"
            };

            var afterSchool1 = new Service
            {
                Id = Guid.NewGuid(),
                Organization = theDen,
                Name = "After School Wrestling 1 Day",
                ShortName = "AfterSchool1Day"
            };

            var afterSchool2 = new Service
            {
                Id = Guid.NewGuid(),
                Organization = theDen,
                Name = "After School Wrestling 2 Day",
                ShortName = "AfterSchool2Day"
            };

            db.Services.AddRange(
                openMat,
                seriousYouth,
                hsWrestling,
                onlineTraining,
                afterSchool1,
                afterSchool2);

            var competitor = new MembershipPlan
            {
                Id = Guid.NewGuid(),
                Organization = wolfpack,
                Name = "Wolfpack Competitor",
                ShortName = "WolfpackCompetitor",
                MonthlyPrice = 379.00m
            };

            var elite = new MembershipPlan
            {
                Id = Guid.NewGuid(),
                Organization = wolfpack,
                Name = "Wolfpack Elite",
                ShortName = "WolfpackElite",
                MonthlyPrice = 479.00m
            };

            var denAfterSchool1 = new MembershipPlan
            {
                Id = Guid.NewGuid(),
                Organization = theDen,
                Name = "The Den After School 1 Day",
                ShortName = "DenAfterSchool1Day",
                MonthlyPrice = 149.00m
            };

            var denAfterSchool2 = new MembershipPlan
            {
                Id = Guid.NewGuid(),
                Organization = theDen,
                Name = "The Den After School 2 Day",
                ShortName = "DenAfterSchool2Day",
                MonthlyPrice = 249.00m
            };

            db.MembershipPlans.AddRange(
                competitor,
                elite,
                denAfterSchool1,
                denAfterSchool2);

            db.PlanServices.AddRange(
                new PlanService { MembershipPlan = competitor, Service = openMat },
                new PlanService { MembershipPlan = competitor, Service = seriousYouth },
                new PlanService { MembershipPlan = competitor, Service = hsWrestling },
                new PlanService { MembershipPlan = competitor, Service = onlineTraining },

                new PlanService { MembershipPlan = elite, Service = openMat },
                new PlanService { MembershipPlan = elite, Service = seriousYouth },
                new PlanService { MembershipPlan = elite, Service = hsWrestling },
                new PlanService { MembershipPlan = elite, Service = onlineTraining },

                new PlanService { MembershipPlan = denAfterSchool1, Service = afterSchool1 },
                new PlanService { MembershipPlan = denAfterSchool2, Service = afterSchool2 }
            );

            await db.SaveChangesAsync();
        }
    }

    private static async Task BackfillOrderSnapshotsAsync(LegacyOSDbContext db)
    {
        var orders = await db.PurchaseOrders.Include(x => x.Family).ThenInclude(x => x.Guardians)
            .Include(x => x.Family).ThenInclude(x => x.Athletes).Include(x => x.Athlete)
            .Include(x => x.Product).Include(x => x.MembershipPlan)
            .Where(x => x.FamilySnapshotJson == "{}" || x.ItemSnapshotJson == "{}").ToListAsync();
        foreach (var order in orders)
        {
            order.FamilySnapshotJson = JsonSerializer.Serialize(new { order.Family.Id, order.Family.FamilyName,
                guardians = order.Family.Guardians.Select(g => new { g.Id, g.FirstName, g.LastName, g.Email, g.Phone }),
                athletes = order.Family.Athletes.Select(a => new { a.Id, a.FirstName, a.LastName, a.DateOfBirth, a.Gender }) });
            if (order.Athlete is not null) order.AthleteSnapshotJson = JsonSerializer.Serialize(new
                { order.Athlete.Id, order.Athlete.FirstName, order.Athlete.LastName, order.Athlete.DateOfBirth, order.Athlete.Gender });
            order.ItemSnapshotJson = order.Product is not null
                ? JsonSerializer.Serialize(new { order.Product.Id, order.Product.Name, order.Product.ShortName, order.Product.Description,
                    order.Product.ProductType, order.Product.Price, order.Product.IsSessionPackage, order.Product.HasUnlimitedSessions,
                    order.Product.SessionCount, order.Product.ValidityDays })
                : JsonSerializer.Serialize(new { order.MembershipPlan!.Id, order.MembershipPlan.Name,
                    order.MembershipPlan.ShortName, order.MembershipPlan.MonthlyPrice });
            order.OriginalAmount = order.Amount + order.DiscountAmount;
        }
        if (orders.Count > 0) await db.SaveChangesAsync();
    }

    private static async Task EnsureBootstrapStaffAsync(IServiceProvider services, LegacyOSDbContext db)
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        var email = configuration["BootstrapStaff:Email"];
        var password = configuration["BootstrapStaff:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return;

        var normalizedEmail = TokenUtilities.NormalizeEmail(email);
        if (await db.PortalUsers.AnyAsync(x => x.NormalizedEmail == normalizedEmail)) return;

        if (password.Length < 12)
            throw new InvalidOperationException("BootstrapStaff:Password must be at least 12 characters.");

        var user = new PortalUser
        {
            Id = Guid.NewGuid(), Email = email.Trim(), NormalizedEmail = normalizedEmail,
            Role = PortalRoles.Staff, CreatedOn = DateTime.UtcNow
        };
        var passwordHasher = services.GetRequiredService<IPasswordHasher<PortalUser>>();
        user.PasswordHash = passwordHasher.HashPassword(user, password);
        db.PortalUsers.Add(user);
        await db.SaveChangesAsync();
    }
}
