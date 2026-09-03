using LegacyOS.Api.Features.Enrollments;
using LegacyOS.Api.Features.Families;
using LegacyOS.Api.Features.Memberships;
using LegacyOS.Api.Features.Organizations;
using Microsoft.EntityFrameworkCore;
using LegacyOS.Api.Features.Products;
using LegacyOS.Api.Features.Activities;
using LegacyOS.Api.Features.Portal;
using LegacyOS.Api.Features.Purchases;
using LegacyOS.Api.Features.UsaWrestling;

namespace LegacyOS.Api.Data;

public class LegacyOSDbContext : DbContext
{
    public LegacyOSDbContext(DbContextOptions<LegacyOSDbContext> options)
        : base(options)
    {
    }

    public DbSet<Family> Families => Set<Family>();
    public DbSet<Guardian> Guardians => Set<Guardian>();
    public DbSet<Athlete> Athletes => Set<Athlete>();

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<FamilyOrganization> FamilyOrganizations => Set<FamilyOrganization>();

    public DbSet<MembershipPlan> MembershipPlans => Set<MembershipPlan>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<PlanService> PlanServices => Set<PlanService>();

    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Activity> Activities => Set<Activity>();

    public DbSet<PortalUser> PortalUsers => Set<PortalUser>();
    public DbSet<PortalAccessToken> PortalAccessTokens => Set<PortalAccessToken>();
    public DbSet<GuardianInvitation> GuardianInvitations => Set<GuardianInvitation>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<UsaWrestlingVerification> UsaWrestlingVerifications => Set<UsaWrestlingVerification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LegacyOSDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
