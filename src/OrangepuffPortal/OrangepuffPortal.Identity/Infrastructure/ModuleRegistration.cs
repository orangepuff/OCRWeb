using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrangepuffPortal.Identity.Domain.Entity;
using OrangepuffPortal.Identity.Domain.Repositories;
using OrangepuffPortal.Identity.Infrastructure.Repositories;
using OrangepuffPortal.Identity.Infrastructure.Seeding;
using OrangepuffPortal.Shared.Modules;

namespace OrangepuffPortal.Identity.Infrastructure;

/// <summary>
/// Composition root entry point for the Identity module.
/// </summary>
public static class ModuleRegistration
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SeedOptions>(configuration.GetSection(SeedOptions.SectionName));

        var connectionString = configuration.GetConnectionString("Portal");
        services.AddDbContext<UserDbContext>(opt =>
            opt.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", UserDbContext.Schema)));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISecurityRuleCategoryRepository, SecurityRuleCategoryRepository>();
        services.AddScoped<ISecurityRuleItemRepository, SecurityRuleItemRepository>();
        services.AddScoped<ISecurityUserRuleItemRepository, SecurityUserRuleItemRepository>();
        services.AddScoped<IUserRegistrationPolicy, ConfigurationUserRegistrationPolicy>();
        services.AddScoped<UserDbSeeder>();
        services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

        services.AddSingleton<IPortalModule, IdentityPortalModule>();

        // MediatR is registered once, for every module's assembly together, at the Host composition root
        // (PortalServiceCollectionExtensions.AddOrangepuffPortal) — Identity's AddUserCommandHandler
        // publishes a notification Config's module needs to receive, so a single IMediator instance has to
        // know about both assemblies' handlers.

        return services;
    }
}
