using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LegacyOS.Api.Data;

public class LegacyOSDbContextFactory : IDesignTimeDbContextFactory<LegacyOSDbContext>
{
    public LegacyOSDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<LegacyOSDbContext>()
            .UseNpgsql("Host=localhost;Database=legacyos;Username=postgres;Password=design-time-only")
            .Options;
        return new LegacyOSDbContext(options);
    }
}
