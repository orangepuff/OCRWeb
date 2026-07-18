using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using System.Security.Claims;

namespace OCRWeb.Bff.Endpoints
{
    /// <summary>
    /// Maps /bff/login, /bff/logout, /bff/me — see docs/Authentication/authentication-design.docx §8.
    /// </summary>
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this WebApplication app)
        {
            app.MapGet("/bff/login", (string? returnUrl, IConfiguration configuration) =>
            {
                var frontendBaseUrl = configuration["Frontend:BaseUrl"] ?? throw new InvalidOperationException("Frontend:BaseUrl is not configured.");

                var safeReturnUrl = IsSafeReturnUrl(returnUrl) ? returnUrl : "/";

                var properties = new AuthenticationProperties
                {
                    RedirectUri = $"{frontendBaseUrl}{safeReturnUrl}",
                };

                return Results.Challenge(properties, [GoogleDefaults.AuthenticationScheme]);
            });

            app.MapPost("/bff/logout", async (HttpContext context) =>
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return Results.NoContent();
            }).RequireAuthorization();

            app.MapGet("/bff/me", (ClaimsPrincipal user) =>
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)!.Value;
                var email = user.FindFirst(ClaimTypes.Email)?.Value;
                var displayName = user.FindFirst(ClaimTypes.Name)?.Value;

                return Results.Ok(new MeResponse(userId, email, displayName));
            }).RequireAuthorization();
        }

        private static bool IsSafeReturnUrl(string? returnUrl) =>
            !string.IsNullOrEmpty(returnUrl) &&
            returnUrl.StartsWith('/') &&
            !returnUrl.StartsWith("//") &&
            !returnUrl.StartsWith("/\\");
    }
}
