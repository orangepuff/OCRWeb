using OrangepuffPortal.Shared.Infrastructure.Design;
using Microsoft.EntityFrameworkCore;

namespace OrangepuffPortal.Theme.Infrastructure;

/// <summary>
/// Design-time factory for <see cref="ThemeDbContext"/> (see the shared base).
/// </summary>
public class ThemeDbContextDesignTimeFactory : DesignTimeDbContextFactoryBase<ThemeDbContext>
{
    protected override string MigrationsHistorySchema => ThemeDbContext.Schema;

    protected override ThemeDbContext Create(DbContextOptions<ThemeDbContext> options) => new(options);
}
