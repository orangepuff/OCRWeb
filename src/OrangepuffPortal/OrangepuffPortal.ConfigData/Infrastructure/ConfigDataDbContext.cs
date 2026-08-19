using Microsoft.EntityFrameworkCore;
using OrangepuffPortal.ConfigData.Domain.Entity;
using OrangepuffPortal.ConfigData.Infrastructure.Configurations;

namespace OrangepuffPortal.ConfigData.Infrastructure;

public class ConfigDataDbContext(DbContextOptions<ConfigDataDbContext> options) : DbContext(options)
{
    public const string Schema = "configdata";

    public DbSet<ConfigDataEntry> ConfigData => Set<ConfigDataEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfiguration(new ConfigDataConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
