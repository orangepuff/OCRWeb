using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OCRWeb.Identity.Domain.Entity;
using OCRWeb.Identity.Domain.Repositories;
using OCRWeb.Identity.Infrastructure.Repositories;
using OCRWeb.Identity.Infrastructure.Seeding;

namespace OCRWeb.Identity.Infrastructure;

/// <summary>
/// Composition root entry point for the Identity module.
/// </summary>
public static class ModuleRegistration
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SeedOptions>(configuration.GetSection(SeedOptions.SectionName));

        var connectionString = configuration.GetConnectionString("OCRWeb");
        services.AddDbContext<UserDbContext>(opt =>
            opt.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", UserDbContext.Schema)));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserRegistrationPolicy, ConfigurationUserRegistrationPolicy>();
        services.AddScoped<UserDbSeeder>();
        services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ModuleRegistration).Assembly));

        return services;
    }
}
