using Microsoft.EntityFrameworkCore;
using OrangepuffPortal.Shared.Infrastructure.Design;

namespace OrangepuffPortal.Config.Infrastructure;

/// <summary>
/// Design-time factory for <see cref="ConfigDbContext"/> (see the shared base).
/// </summary>
public class ConfigDbContextDesignTimeFactory : DesignTimeDbContextFactoryBase<ConfigDbContext>
{
    protected override string MigrationsHistorySchema => ConfigDbContext.Schema;

    protected override ConfigDbContext Create(DbContextOptions<ConfigDbContext> options) => new(options);
}
