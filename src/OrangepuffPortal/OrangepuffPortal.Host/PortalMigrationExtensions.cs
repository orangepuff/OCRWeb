using OrangepuffPortal.Shared.Modules;

namespace OrangepuffPortal.Host
{
    /// <summary>
    /// Runs migration + seed for every registered <see cref="IPortalModule"/> (Identity's own, plus any body-app module that registers itself the same way), gated by a single DoMigration flag.
    /// The database itself is assumed to exist; migrations only create/upgrade schema and tables. Modules run in DI-registration order.
    /// </summary>
    public static class PortalMigrationExtensions
    {
        public static async Task MigratePortalModulesAsync(this WebApplication app, CancellationToken cancellationToken = default)
        {
            if (!app.Configuration.GetValue<bool>("DoMigration"))
            {
                return;
            }

            using var scope = app.Services.CreateScope();
            foreach (var module in scope.ServiceProvider.GetServices<IPortalModule>())
            {
                await module.MigrateAsync(scope.ServiceProvider, cancellationToken);
                await module.SeedAsync(scope.ServiceProvider, cancellationToken);
            }
        }
    }
}
