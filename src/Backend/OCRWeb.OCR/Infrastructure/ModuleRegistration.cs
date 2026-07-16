using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OCRWeb.OCR.Infrastructure;

/// <summary>
/// Composition root entry point for the OCR module.
/// </summary>
public static class ModuleRegistration
{
    public static IServiceCollection AddOcr(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("OCRWeb");
        services.AddDbContext<OcrDbContext>(opt =>
            opt.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", OcrDbContext.Schema)));

        // Register this module's MediatR command/query handlers as they are added.
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ModuleRegistration).Assembly));

        return services;
    }
}
