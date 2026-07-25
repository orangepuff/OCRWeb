using Microsoft.EntityFrameworkCore;
using OCRWeb.ProjectManagement.Domain.Entity;
using OCRWeb.ProjectManagement.Infrastructure.Configurations;

namespace OCRWeb.ProjectManagement.Infrastructure;

/// <summary>
/// EF Core context for the Project Management bounded context. Owns the [project] schema.
/// </summary>
public class ProjectDbContext(DbContextOptions<ProjectDbContext> options)
    : DbContext(options)
{
    public const string Schema = "project";

    public DbSet<Project> Projects => Set<Project>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfiguration(new ProjectConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
