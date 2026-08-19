using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrangepuffPortal.ConfigData.Contract.Interfaces;
using OrangepuffPortal.ConfigData.Domain.Repositories;
using OrangepuffPortal.ConfigData.Infrastructure.Repositories;
using OrangepuffPortal.Shared.Modules;

namespace OrangepuffPortal.ConfigData.Infrastructure;

public static class ModuleRegistration
{
    public static IServiceCollection AddConfigDataModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Portal");
        services.AddDbContext<ConfigDataDbContext>(opt =>
            opt.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", ConfigDataDbContext.Schema)));

        services.AddScoped<IConfigDataRepository, ConfigDataRepository>();
        services.AddScoped<IConfigDataAdminService, ConfigDataAdminService>();
        services.AddScoped<IConfigDataService, ConfigDataService>();

        services.AddSingleton<ConfigDataCache>();
        services.AddHostedService<ConfigDataCacheWarmer>();

        services.AddSingleton<IPortalModule, ConfigDataPortalModule>();
        return services;
    }
}
