using Microsoft.EntityFrameworkCore;
using OrangepuffPortal.ConfigText.Domain.Entity;
using OrangepuffPortal.ConfigText.Infrastructure.Configurations;

namespace OrangepuffPortal.ConfigText.Infrastructure;

/// <summary>
/// EF Core context for the shared UI text store. Owns the [configtext] schema.
/// </summary>
public class ConfigTextDbContext(DbContextOptions<ConfigTextDbContext> options) : DbContext(options)
{
    public const string Schema = "configtext";

    public DbSet<ConfigTextDefinition> ConfigTextDefinitions => Set<ConfigTextDefinition>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfiguration(new ConfigTextDefinitionConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
