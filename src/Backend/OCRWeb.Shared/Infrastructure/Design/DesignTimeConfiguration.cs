using Microsoft.Extensions.Configuration;

namespace OCRWeb.Shared.Infrastructure.Design;

/// <summary>
/// Resolves configuration for design-time EF tooling. Reads the connection string from the
/// startup project's appsettings (run dotnet ef with -s src/Backend/OCRWeb.API), then from
/// environment variables. Used only by <see cref="DesignTimeDbContextFactoryBase{TContext}"/>.
/// </summary>
public static class DesignTimeConfiguration
{
    public static string GetConnectionString(string name = "OCRWeb")
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
                $"Connection string '{name}' was not found at design time. Run dotnet ef with the API as " +
                "the startup project (-s src/Backend/OCRWeb.API) so its appsettings is on the base path, " +
                $"or set the ConnectionStrings__{name} environment variable.");
        }

        return connectionString;
    }
}
