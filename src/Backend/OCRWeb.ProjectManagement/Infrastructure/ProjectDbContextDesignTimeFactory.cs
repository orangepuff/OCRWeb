using Microsoft.EntityFrameworkCore;
using OrangepuffPortal.Shared.Infrastructure.Design;

namespace OCRWeb.ProjectManagement.Infrastructure;

/// <summary>Design-time factory for <see cref="ProjectDbContext"/> (see the shared base).</summary>
public class ProjectDbContextDesignTimeFactory
    : DesignTimeDbContextFactoryBase<ProjectDbContext>
{
    protected override string MigrationsHistorySchema => ProjectDbContext.Schema;

    protected override ProjectDbContext Create(DbContextOptions<ProjectDbContext> options)
        => new(options);
}
