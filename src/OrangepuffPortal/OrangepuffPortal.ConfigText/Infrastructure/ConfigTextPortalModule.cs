using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrangepuffPortal.Shared.Modules;

namespace OrangepuffPortal.ConfigText.Infrastructure;

/// <summary>
/// <see cref="IPortalModule"/> implementation for the ConfigText module — applies
/// <see cref="ConfigTextDbContext"/>'s migrations. Seeding is each consuming app's own responsibility through <see cref="Contract.Interfaces.IConfigTextWriter"/>, not this module's <c>SeedAsync</c>.
/// </summary>
internal sealed class ConfigTextPortalModule : IPortalModule
{
    public string Name => "ConfigText";

    public async Task MigrateAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default) => await serviceProvider.GetRequiredService<ConfigTextDbContext>().Database.MigrateAsync(cancellationToken);
}
