using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OCRWeb.DocumentProcessing.Application.Interfaces;
using OCRWeb.DocumentProcessing.Domain.Repositories;
using OCRWeb.DocumentProcessing.Infrastructure.Repositories;

namespace OCRWeb.DocumentProcessing.Infrastructure;

/// <summary>
/// Composition root entry point for the Document Processing module.
/// </summary>
public static class ModuleRegistration
{
    public static IServiceCollection AddDocumentProcessing(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("OCRWeb");
        services.AddDbContext<DocumentProcessingDbContext>(opt =>
            opt.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", DocumentProcessingDbContext.Schema)));

        services.AddScoped<IPdfFileRepository, PdfFileRepository>();
        services.AddSingleton<IPdfManipulator, PdfSharpManipulator>();

        // Register this module's MediatR command/query handlers.
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ModuleRegistration).Assembly));

        return services;
    }
}
