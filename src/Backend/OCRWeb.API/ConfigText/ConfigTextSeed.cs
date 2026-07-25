using System.Reflection;
using System.Text.Json;
using OCRWeb.Document.Api;
using OCRWeb.ProjectManagement.Api;
using OrangepuffPortal.ConfigText.Contract;

namespace OCRWeb.API.ConfigText;

/// <summary>
/// Loads every module's own default text from its embedded ConfigText/{culture}.json seed file.
/// See docs/config-text-consumption.md for the seed-file convention and why this exists.
/// </summary>
public static class ConfigTextSeed
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private static readonly Assembly[] ModuleAssemblies =
    [
        Assembly.GetExecutingAssembly(),
        typeof(ProjectManagementApiMarker).Assembly,
        typeof(DocumentApiMarker).Assembly,
    ];

    /// <summary>Reads every module's ConfigText/{cultureCode}.json embedded resource, if it has one.</summary>
    public static IReadOnlyCollection<ConfigTextSeedEntry> LoadAll(string cultureCode)
    {
        var entries = new List<ConfigTextSeedEntry>();

        foreach (var assembly in ModuleAssemblies)
        {
            var resourceName = assembly.GetManifestResourceNames()
                .SingleOrDefault(name => name.EndsWith($"ConfigText.{cultureCode}.json", StringComparison.OrdinalIgnoreCase));

            if (resourceName is null)
            {
                continue;
            }

            using var stream = assembly.GetManifestResourceStream(resourceName)!;
            var moduleEntries = JsonSerializer.Deserialize<List<ConfigTextSeedEntry>>(stream, JsonOptions)
                ?? throw new InvalidOperationException($"{resourceName} deserialized to null.");

            entries.AddRange(moduleEntries);
        }

        return entries;
    }
}
