using Microsoft.EntityFrameworkCore;
using OrangepuffPortal.Theme.Domain.Entity;
using OrangepuffPortal.Theme.Infrastructure.Configurations;

namespace OrangepuffPortal.Theme.Infrastructure;

/// <summary>
/// EF Core context for the theme store. Owns the [portal] schema.
/// </summary>
public class ThemeDbContext(DbContextOptions<ThemeDbContext> options) : DbContext(options)
{
    public const string Schema = "portal";

    public DbSet<Domain.Entity.Theme> Themes => Set<Domain.Entity.Theme>();
    public DbSet<ThemeSection> ThemeSections => Set<ThemeSection>();
    public DbSet<ThemeElement> ThemeElements => Set<ThemeElement>();
    public DbSet<ThemeDetail> ThemeDetails => Set<ThemeDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfiguration(new ThemeConfiguration());
        modelBuilder.ApplyConfiguration(new ThemeSectionConfiguration());
        modelBuilder.ApplyConfiguration(new ThemeElementConfiguration());
        modelBuilder.ApplyConfiguration(new ThemeDetailConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
