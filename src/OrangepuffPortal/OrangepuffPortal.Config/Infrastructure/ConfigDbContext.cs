using Microsoft.EntityFrameworkCore;
using OrangepuffPortal.Config.Domain.Entity;
using OrangepuffPortal.Config.Infrastructure.Configurations;

namespace OrangepuffPortal.Config.Infrastructure;

/// <summary>
/// EF Core context for the per-user settings store.
/// Owns the [config] schema.
/// </summary>
public class ConfigDbContext(DbContextOptions<ConfigDbContext> options) : DbContext(options)
{
    public const string Schema = "config";

    public DbSet<ConfigSection> ConfigSections => Set<ConfigSection>();
    public DbSet<ConfigItem> Configs => Set<ConfigItem>();
    public DbSet<ConfigUser> ConfigUsers => Set<ConfigUser>();
    public DbSet<ConfigUserHistory> ConfigUsersHistory => Set<ConfigUserHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfiguration(new ConfigSectionConfiguration());
        modelBuilder.ApplyConfiguration(new ConfigItemConfiguration());
        modelBuilder.ApplyConfiguration(new ConfigUserConfiguration());
        modelBuilder.ApplyConfiguration(new ConfigUserHistoryConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
