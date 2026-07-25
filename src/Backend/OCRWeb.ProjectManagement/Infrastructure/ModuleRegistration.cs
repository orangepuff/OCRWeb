using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OCRWeb.ProjectManagement.Domain.Repositories;
using OCRWeb.ProjectManagement.Infrastructure.Repositories;

namespace OCRWeb.ProjectManagement.Infrastructure;

/// <summary>
/// Composition root entry point for the Project Management module.
/// </summary>
public static class ModuleRegistration
{
    public static IServiceCollection AddProjectManagement(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("OCRWeb");
        services.AddDbContext<ProjectDbContext>(opt =>
            opt.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", ProjectDbContext.Schema)));

        services.AddScoped<IProjectRepository, ProjectRepository>();

        // Register this module's MediatR command/query handlers.
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ModuleRegistration).Assembly));

        return services;
    }
}
