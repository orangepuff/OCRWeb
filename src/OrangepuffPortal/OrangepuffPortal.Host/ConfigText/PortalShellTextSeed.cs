using System.Reflection;
using System.Text.Json;
using OrangepuffPortal.ConfigText.Contract;

namespace OrangepuffPortal.Host.ConfigText;

/// <summary>
/// Loads the portal shell's own default text (shared "Common" words, admin-screen labels/messages,
/// and every backend module's rejection-reason messages) from the embedded
/// <c>ConfigText/{culture}.json</c> seed file — same convention as a consuming app's own module seed
/// files (see e.g. OCRWeb.API's <c>ConfigTextSeed</c>), except this one ships with the portal itself
/// so every consuming app gets it automatically without having to remember to include it.
/// </summary>
internal static class PortalShellTextSeed
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Reads this assembly's ConfigText/{cultureCode}.json embedded resource, if it has one.</summary>
    public static IReadOnlyCollection<ConfigTextSeedEntry> Load(string cultureCode)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .SingleOrDefault(name => name.EndsWith($"ConfigText.{cultureCode}.json", StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
        {
            return [];
        }

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        return JsonSerializer.Deserialize<List<ConfigTextSeedEntry>>(stream, JsonOptions)
            ?? throw new InvalidOperationException($"{resourceName} deserialized to null.");
    }
}
