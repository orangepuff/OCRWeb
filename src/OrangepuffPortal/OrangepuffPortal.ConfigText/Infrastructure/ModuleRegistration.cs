using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrangepuffPortal.ConfigText.Contract.Interfaces;
using OrangepuffPortal.ConfigText.Domain.Repositories;
using OrangepuffPortal.ConfigText.Infrastructure.Repositories;
using OrangepuffPortal.Shared.Modules;
using OrangepuffPortal.Shared.Translation;

namespace OrangepuffPortal.ConfigText.Infrastructure;

/// <summary>
/// Composition root entry point for the ConfigText module.
/// </summary>
public static class ModuleRegistration
{
    public static IServiceCollection AddConfigTextModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Portal");
        services.AddDbContext<ConfigTextDbContext>(opt =>
            opt.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", ConfigTextDbContext.Schema)));

        // IDistributedCache (Redis or fallback) is registered by AddOrangepuffPortal in OrangepuffPortal.Host.
        services.AddScoped<IConfigTextRepository, ConfigTextRepository>();
        services.AddSingleton<ConfigTextCache>();
        services.AddScoped<IConfigTextWriter, ConfigTextWriter>();
        services.AddScoped<IConfigTextReader, ConfigTextReader>();
        services.AddScoped<IConfigTextAdminService, ConfigTextAdminService>();
        services.AddScoped<ITranslation, Translation>();

        services.AddSingleton<IPortalModule, ConfigTextPortalModule>();

        return services;
    }
}
