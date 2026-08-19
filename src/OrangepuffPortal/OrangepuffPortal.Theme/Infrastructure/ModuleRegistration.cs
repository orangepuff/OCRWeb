using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrangepuffPortal.Theme.Contract.Interfaces;
using OrangepuffPortal.Theme.Domain.Repositories;
using OrangepuffPortal.Theme.Infrastructure.Repositories;
using OrangepuffPortal.Theme.Infrastructure.Seeding;
using OrangepuffPortal.Shared.Modules;

namespace OrangepuffPortal.Theme.Infrastructure;

/// <summary>
/// Composition root entry point for the Theme module.
/// </summary>
public static class ModuleRegistration
{
    public static IServiceCollection AddThemeModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Portal");
        services.AddDbContext<ThemeDbContext>(opt =>
            opt.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", ThemeDbContext.Schema)));

        services.AddScoped<IThemeRepository, ThemeRepository>();
        services.AddScoped<ThemeDbSeeder>();

        services.AddMemoryCache();
        services.AddSingleton<UserThemeCache>();
        services.AddScoped<ICurrentUserTheme, CurrentUserTheme>();
        services.AddScoped<IUserThemeCacheWarmer, UserThemeCacheWarmer>();

        services.AddSingleton<IPortalModule, ThemePortalModule>();

        return services;
    }
}
