using Microsoft.EntityFrameworkCore;
using OrangepuffPortal.Shared.Infrastructure.Design;

namespace OrangepuffPortal.ConfigText.Infrastructure;

/// <summary>
/// Design-time factory for <see cref="ConfigTextDbContext"/> (see the shared base).
/// </summary>
public class ConfigTextDbContextDesignTimeFactory : DesignTimeDbContextFactoryBase<ConfigTextDbContext>
{
    protected override string MigrationsHistorySchema => ConfigTextDbContext.Schema;

    protected override ConfigTextDbContext Create(DbContextOptions<ConfigTextDbContext> options) => new(options);
}
