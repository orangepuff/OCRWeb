using System.Security.Claims;
using OrangepuffPortal.Theme.Contract;
using OrangepuffPortal.Theme.Contract.Interfaces;
using OrangepuffPortal.Theme.Domain.Repositories;

namespace OrangepuffPortal.Bff.Endpoints.ThemeUserEndpoints;

/// <summary>
/// Maps /bff/themes (active theme list), /bff/theme (current user's CSS-var map), /bff/me/theme (theme selection).
/// </summary>
public static class ThemeUserEndpoints
{
    public static void MapThemeUserEndpoints(this WebApplication app)
    {
        // Any authenticated user — lists active themes for the theme-selector UI.
        app.MapGet("/bff/themes", async (IThemeRepository repo, CancellationToken ct) =>
        {
            var themes = await repo.ListThemesAsync(ct);
            var result = themes
                .Where(t => t.IsActive)
                .OrderBy(t => t.ThemeCode)
                .Select(t => new ThemeSummaryResponse(t.Id, t.ThemeCode, t.Description));
            return Results.Ok(result);
        }).RequireAuthorization();

        // Returns the calling user's resolved theme as a flat CSS custom-property map.
        // Re-warms cache on miss so the response is never empty after a pod restart.
        app.MapGet("/bff/theme", async (
            ClaimsPrincipal user,
            ICurrentUserTheme currentUserTheme,
            IUserThemeCacheWarmer cacheWarmer,
            CancellationToken ct) =>
        {
            var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var theme = currentUserTheme.GetTheme();

            if (theme.Id == 0)
            {
                await cacheWarmer.WarmAsync(userId, ct);
                theme = currentUserTheme.GetTheme();
            }

            var cssVars = ThemeCssVarBuilder.Build(theme);
            return Results.Ok(new ThemeVarsResponse(theme.Id, theme.ThemeCode, theme.Description, cssVars));
        }).RequireAuthorization();

        // Persists the user's preferred theme, re-warms cache; frontend then calls GET /bff/theme to re-apply.
        app.MapPut("/bff/me/theme", async (
            SelectThemeRequest request,
            ClaimsPrincipal user,
            IUserThemeSelector selector,
            CancellationToken ct) =>
        {
            var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            try
            {
                await selector.SelectThemeAsync(userId, request.ThemeId, ct);
                return Results.NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).RequireAuthorization();
    }

    private record ThemeSummaryResponse(int Id, string ThemeCode, string Description);
    private record ThemeVarsResponse(int ThemeId, string ThemeCode, string Description, Dictionary<string, string> CssVars);
    private record SelectThemeRequest(int ThemeId);
}
