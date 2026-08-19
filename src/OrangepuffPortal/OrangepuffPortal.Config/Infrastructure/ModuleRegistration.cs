using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrangepuffPortal.Config.Contract.Interfaces;
using OrangepuffPortal.Config.Domain.Repositories;
using OrangepuffPortal.Config.Infrastructure.Repositories;
using OrangepuffPortal.Shared.Modules;

namespace OrangepuffPortal.Config.Infrastructure;

/// <summary>
/// Composition root entry point for the Config module.
/// </summary>
public static class ModuleRegistration
{
    public static IServiceCollection AddConfigModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Portal");
        services.AddDbContext<ConfigDbContext>(opt =>
            opt.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", ConfigDbContext.Schema)));

        services.AddScoped<IConfigRepository, ConfigRepository>();
        services.AddScoped<IConfigCatalogWriter, ConfigCatalogWriter>();
        services.AddScoped<IConfigCatalogAdminService, ConfigCatalogAdminService>();
        services.AddScoped<IConfigUserValueService, ConfigUserValueService>();

        // UserConfigCache uses IMemoryCache as an L1 (synchronous) layer; IDistributedCache (Redis or
        // fallback) is the L2 layer registered by AddOrangepuffPortal in OrangepuffPortal.Host.
        services.AddMemoryCache();
        services.AddSingleton<UserConfigCache>();
        services.AddScoped<ICurrentUserConfig, CurrentUserConfig>();
        services.AddScoped<IUserConfigCacheWarmer, UserConfigCacheWarmer>();

        services.AddSingleton<IPortalModule, ConfigPortalModule>();

        return services;
    }
}
