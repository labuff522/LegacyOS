using LegacyOS.Api.Features.Families;
using LegacyOS.Api.Features.Organizations;
using Microsoft.EntityFrameworkCore;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LegacyOSDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}