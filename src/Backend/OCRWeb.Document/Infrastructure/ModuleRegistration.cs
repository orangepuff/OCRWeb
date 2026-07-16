using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OCRWeb.Document.Domain.Repositories;
using OCRWeb.Document.Infrastructure.Repositories;

namespace OCRWeb.Document.Infrastructure;

/// <summary>
/// Composition root entry point for the Document Processing module.
/// </summary>
public static class ModuleRegistration
{
    public static IServiceCollection AddDocument(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("OCRWeb");
        services.AddDbContext<DocumentDbContext>(opt =>
            opt.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", DocumentDbContext.Schema)));

        services.AddScoped<IPdfFileRepository, PdfFileRepository>();

        // Register this module's MediatR command/query handlers.
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ModuleRegistration).Assembly));

        return services;
    }
}
