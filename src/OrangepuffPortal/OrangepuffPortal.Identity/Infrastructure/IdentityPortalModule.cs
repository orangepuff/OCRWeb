using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrangepuffPortal.Identity.Infrastructure.Seeding;
using OrangepuffPortal.Shared.Modules;

namespace OrangepuffPortal.Identity.Infrastructure;

/// <summary>
/// <see cref="IPortalModule"/> implementation for the Identity module — applies
/// <see cref="UserDbContext"/>'s migrations and seeds the default admin user.
/// </summary>
internal sealed class IdentityPortalModule : IPortalModule
{
    public string Name => "Identity";

    public async Task MigrateAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default) => await serviceProvider.GetRequiredService<UserDbContext>().Database.MigrateAsync(cancellationToken);

    public async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default) => await serviceProvider.GetRequiredService<UserDbSeeder>().SeedAsync(cancellationToken);
}
