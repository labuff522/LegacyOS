using Microsoft.EntityFrameworkCore;

namespace LegacyOS.Api.Data;

public class LegacyOSDbContext : DbContext
{
    public LegacyOSDbContext(DbContextOptions<LegacyOSDbContext> options)
        : base(options)
    {
    }
}