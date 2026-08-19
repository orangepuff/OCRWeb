using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrangepuffPortal.Shared.Modules;

namespace OrangepuffPortal.Config.Infrastructure;

/// <summary>
/// <see cref="IPortalModule"/> implementation for the Config module — applies
/// <see cref="ConfigDbContext"/>'s migrations. Catalog seeding is each consuming app's own
/// responsibility through <see cref="Contract.Interfaces.IConfigCatalogWriter"/>, not this module's
/// <c>SeedAsync</c>; ConfigUsers/ConfigUsersHistory are never seeded at all.
/// </summary>
internal sealed class ConfigPortalModule : IPortalModule
{
    public string Name => "Config";

    public async Task MigrateAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default) =>
        await serviceProvider.GetRequiredService<ConfigDbContext>().Database.MigrateAsync(cancellationToken);
}
