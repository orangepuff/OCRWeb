using OrangepuffPortal.ConfigText.Contract.Interfaces;

namespace OrangepuffPortal.Bff.Endpoints.ConfigTextEndpoints
{
    /// <summary>
    /// Maps /bff/config-text.
    /// </summary>
    public static class ConfigTextEndpoints
    {
        public static void MapConfigTextEndpoints(this WebApplication app)
        {
            // Deliberately anonymous: this is UI label/message text, not sensitive data (see
            // docs/config-text-design.md), and the frontend's TranslationService needs it preloaded
            // before it knows whether there's a signed-in session — e.g. the landing/sign-in page.
            app.MapGet("/bff/config-text", async (string culture, string? modules, IConfigTextReader reader, CancellationToken ct) =>
            {
                var moduleList = string.IsNullOrWhiteSpace(modules)
                    ? null
                    : modules.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                var entries = await reader.GetAllAsync(culture, moduleList, ct);
                return Results.Ok(entries);
            });
        }
    }
}
