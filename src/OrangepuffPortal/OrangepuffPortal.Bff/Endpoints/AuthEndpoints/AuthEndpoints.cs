using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using OrangepuffPortal.Bff.Infrastructure;
using OrangepuffPortal.Bff.Infrastructure.IdentityGateway;
using OrangepuffPortal.Config.Contract.Interfaces;
using OrangepuffPortal.Theme.Contract.Interfaces;
using OrangepuffPortal.Shared.Auditing;
using System.Security.Claims;

namespace OrangepuffPortal.Bff.Endpoints.AuthEndpoints
{
    /// <summary>
    /// Maps /bff/login, /bff/login/password, /bff/logout, /bff/me, /bff/me/permissions,
    /// /bff/me/display-name, /bff/me/password.
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
                    // Without this the cookie is session-only — it survives sliding revalidation but is dropped the moment the browser closes, even though ExpireTimeSpan says 8 hours.
                    IsPersistent = true,
                };

                return Results.Challenge(properties, [GoogleDefaults.AuthenticationScheme]);
            });

            app.MapPost("/bff/login/password", async (PasswordSignInRequest request, IIdentityGateway client, HttpContext context, IUserConfigCacheWarmer configWarmer, IUserThemeCacheWarmer themeWarmer, CancellationToken ct) =>
            {
                var result = await client.VerifyPasswordAsync(request.UsernameOrEmail, request.Password, ct);
                if (!result.Success)
                {
                    return Results.Unauthorized();
                }

                // Same claim shape as the Google flow's OnCreatingTicket (PortalBffServiceCollectionExtensions)
                // so /bff/me, the AdminOnly policy, and OnValidatePrincipal's periodic re-check all behave
                // identically regardless of which flow signed the user in.
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, result.UserId!.Value.ToString()),
                    new(PortalClaimTypes.CultureCode, result.CultureCode!),
                    new("lv", DateTimeOffset.UtcNow.ToString("O")),
                };

                if (!string.IsNullOrEmpty(result.Email))
                {
                    claims.Add(new Claim(ClaimTypes.Email, result.Email));
                }
                if (!string.IsNullOrEmpty(result.DisplayName))
                {
                    claims.Add(new Claim(ClaimTypes.Name, result.DisplayName));
                }
                if (await client.IsUserAdminAsync(result.UserId!.Value, ct))
                {
                    claims.Add(new Claim(PortalBffConstants.AdminClaimType, "true"));
                }

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), new AuthenticationProperties { IsPersistent = true });

                await configWarmer.WarmAsync(result.UserId!.Value, ct);
                await themeWarmer.WarmAsync(result.UserId!.Value, ct);

                return Results.NoContent();
            });

            app.MapPost("/bff/logout", async (HttpContext context) =>
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return Results.NoContent();
            }).RequireAuthorization();

            app.MapGet("/bff/me", async (ClaimsPrincipal user, IIdentityGateway client, CancellationToken ct) =>
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)!.Value;
                var email = user.FindFirst(ClaimTypes.Email)?.Value;
                var displayName = user.FindFirst(ClaimTypes.Name)?.Value;
                var cultureCode = user.FindFirst(PortalClaimTypes.CultureCode)?.Value ?? "en-US";
                var isAdmin = await client.IsUserAdminAsync(int.Parse(userId), ct);

                return Results.Ok(new MeResponse(userId, email, displayName, isAdmin, cultureCode));
            }).RequireAuthorization();

            app.MapGet("/bff/me/permissions", async (ClaimsPrincipal user, IIdentityGateway client, CancellationToken ct) =>
            {
                var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var permissions = await client.GetEffectivePermissionsAsync(userId, ct);
                return Results.Ok(permissions);
            }).RequireAuthorization();

            app.MapPut("/bff/me/display-name", async (UpdateDisplayNameRequest request, ClaimsPrincipal user, HttpContext context, IIdentityGateway client, CancellationToken ct) =>
            {
                var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var result = await client.UpdateDisplayNameAsync(userId, request.DisplayName, ct);
                if (!result.Success)
                {
                    return Results.Ok(result);
                }

                // /bff/me reads the Name claim, not the DB — without re-issuing the cookie here, the
                // change wouldn't show up until the next login or OnValidatePrincipal's 5-minute cycle
                // (which only ever re-checks admin/active, never Name).
                var identity = (ClaimsIdentity)user.Identity!;
                var existingNameClaim = identity.FindFirst(ClaimTypes.Name);
                if (existingNameClaim is not null)
                {
                    identity.RemoveClaim(existingNameClaim);
                }
                identity.AddClaim(new Claim(ClaimTypes.Name, request.DisplayName));

                var authenticateResult = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, user, authenticateResult.Properties);

                return Results.Ok(result);
            }).RequireAuthorization();

            app.MapPut("/bff/me/password", async (ChangeOwnPasswordRequest request, ClaimsPrincipal user, IIdentityGateway client, CancellationToken ct) =>
            {
                var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var result = await client.ChangeOwnPasswordAsync(userId, request.CurrentPassword, request.NewPassword, ct);
                return Results.Ok(result);
            }).RequireAuthorization();
        }

        private static bool IsSafeReturnUrl(string? returnUrl) =>
            !string.IsNullOrEmpty(returnUrl) &&
            returnUrl.StartsWith('/') &&
            !returnUrl.StartsWith("//") &&
            !returnUrl.StartsWith("/\\");
    }
}
