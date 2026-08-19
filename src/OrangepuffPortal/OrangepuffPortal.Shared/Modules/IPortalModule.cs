namespace OrangepuffPortal.Shared.Modules;

/// <summary>
/// A module that OrangepuffPortal.Host can migrate and seed generically at startup.
/// Implemented by Identity and by any body-app module that wants to participate in the
/// same DoMigration-gated startup pipeline.
/// </summary>
public interface IPortalModule
{
    /// <summary>Short human-readable name, used only for logging.</summary>
    string Name { get; }

    /// <summary>Apply this module's own EF Core migrations. Resolve its DbContext from <paramref name="serviceProvider"/>.</summary>
    Task MigrateAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default);

    /// <summary>Apply this module's seed data, if any. No-op by default.</summary>
    Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
