using Microsoft.Extensions.Configuration;

namespace OrangepuffPortal.Shared.Infrastructure.Design;

/// <summary>
/// Resolves configuration for design-time EF tooling. Reads the connection string from the
/// startup project's appsettings (run dotnet ef with -s pointing at a host project that defines
/// ConnectionStrings:Portal), then from environment variables. Used only by
/// <see cref="DesignTimeDbContextFactoryBase{TContext}"/>.
/// </summary>
public static class DesignTimeConfiguration
{
    public static string GetConnectionString(string name = "Portal")
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString(name);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{name}' was not found at design time. Run dotnet ef with a " +
                "startup project (-s) whose appsettings.json defines ConnectionStrings:" + name + ", " +
                $"or set the ConnectionStrings__{name} environment variable.");
        }

        return connectionString;
    }
}
