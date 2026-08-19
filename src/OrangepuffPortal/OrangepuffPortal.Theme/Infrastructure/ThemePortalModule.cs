using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrangepuffPortal.Theme.Infrastructure.Seeding;
using OrangepuffPortal.Shared.Modules;

namespace OrangepuffPortal.Theme.Infrastructure;

/// <summary>
/// <see cref="IPortalModule"/> implementation for the Theme module — applies
/// <see cref="ThemeDbContext"/>'s migrations and seeds the Default theme.
/// </summary>
internal sealed class ThemePortalModule : IPortalModule
{
    public string Name => "Theme";

    public async Task MigrateAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default) =>
        await serviceProvider.GetRequiredService<ThemeDbContext>().Database.MigrateAsync(cancellationToken);

    public async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default) =>
        await serviceProvider.GetRequiredService<ThemeDbSeeder>().SeedAsync(cancellationToken);
}
