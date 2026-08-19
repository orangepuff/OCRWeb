using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrangepuffPortal.Shared.Infrastructure.Design;

/// <summary>
/// Shared base for module design-time DbContext factories. Handles connection-string resolution
/// (from appsettings) and the per-context migrations history table, so each module only supplies
/// its schema name and how to construct its context.
/// </summary>
public abstract class DesignTimeDbContextFactoryBase<TContext> : IDesignTimeDbContextFactory<TContext>
    where TContext : DbContext
{
    /// <summary>Schema that holds this context's __EFMigrationsHistory table.</summary>
    protected abstract string MigrationsHistorySchema { get; }

    /// <summary>Construct the concrete context from the built options.</summary>
    protected abstract TContext Create(DbContextOptions<TContext> options);

    public TContext CreateDbContext(string[] args)
    {
        var connectionString = DesignTimeConfiguration.GetConnectionString("Portal");

        var options = new DbContextOptionsBuilder<TContext>()
            .UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", MigrationsHistorySchema))
            .Options;

        return Create(options);
    }
}
