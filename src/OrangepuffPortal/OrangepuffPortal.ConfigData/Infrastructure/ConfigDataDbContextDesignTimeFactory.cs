using OrangepuffPortal.Shared.Infrastructure.Design;

namespace OrangepuffPortal.ConfigData.Infrastructure;

public class ConfigDataDbContextDesignTimeFactory : DesignTimeDbContextFactoryBase<ConfigDataDbContext>
{
    protected override string MigrationsHistorySchema => ConfigDataDbContext.Schema;

    protected override ConfigDataDbContext Create(Microsoft.EntityFrameworkCore.DbContextOptions<ConfigDataDbContext> options) => new(options);
}
