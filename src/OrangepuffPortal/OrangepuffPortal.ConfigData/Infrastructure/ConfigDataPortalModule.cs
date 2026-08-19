using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrangepuffPortal.Shared.Modules;

namespace OrangepuffPortal.ConfigData.Infrastructure;

internal sealed class ConfigDataPortalModule : IPortalModule
{
    public string Name => "ConfigData";

    public async Task MigrateAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default) => await serviceProvider.GetRequiredService<ConfigDataDbContext>().Database.MigrateAsync(cancellationToken);
}
