using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using OrangepuffPortal.Bff.Infrastructure;
using OrangepuffPortal.Bff.Infrastructure.ConfigDataGateway;
using OrangepuffPortal.Bff.Infrastructure.ConfigGateway;
using OrangepuffPortal.Bff.Infrastructure.ConfigTextGateway;
using OrangepuffPortal.Bff.Infrastructure.IdentityGateway;
using OrangepuffPortal.Config.Contract.Interfaces;
using OrangepuffPortal.Theme.Contract.Interfaces;
using OrangepuffPortal.Shared.Auditing;
using System.Security.Claims;

namespace OrangepuffPortal.Bff
{
    /// <summary>
    /// Registers Portal's Bff-owned auth: cookie + Google OAuth schemes, the AdminOnly policy, Data Protection key persistence, and the in-process <see cref="IIdentityGateway"/>.
    /// </summary>
    public static class PortalBffServiceCollectionExtensions
    {
        public static IServiceCollection AddPortalBff(this IServiceCollection services, IConfiguration configuration)
        {
            var appName = configuration["Portal:AppName"] ?? throw new InvalidOperationException("Portal:AppName is not configured.");

            // Persisted to a configurable path (Portal:DataProtection:KeysPath) so keys survive restarts. Defaults to /keys for Docker (volume-mounted from docker-compose.yml); override in appsettings.Development.json for local VS runs.
            var keysPath = configuration["Portal:DataProtection:KeysPath"] ?? "/keys";
            services.AddDataProtection()
                .SetApplicationName(appName)
                .PersistKeysToFileSystem(new DirectoryInfo(keysPath));

            services.AddScoped<IIdentityGateway, IdentityGateway>();
            services.AddScoped<IConfigGateway, ConfigGateway>();
            services.AddScoped<IConfigTextGateway, ConfigTextGateway>();
            services.AddScoped<IConfigDataGateway, ConfigDataGateway>();

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.Cookie.Name = $".{appName}.Auth";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);

                    // /bff/me and /bff/logout are JSON-style endpoints — return plain status codes, never redirect to a login page (the cookie handler's default challenge behavior).
                    options.Events.OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    };
                    options.Events.OnRedirectToAccessDenied = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    };
                    options.Events.OnValidatePrincipal = async context =>
                    {
                        var issuedUtc = context.Properties.IssuedUtc;
                        if (issuedUtc is not null && DateTimeOffset.UtcNow - issuedUtc.Value > TimeSpan.FromDays(30))
                        {
                            context.RejectPrincipal();
                            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                            return;
                        }

                        var lastValidatedClaim = context.Principal?.FindFirst("lv");
                        var lastValidated = lastValidatedClaim is not null
                            ? DateTimeOffset.Parse(lastValidatedClaim.Value)
                            : DateTimeOffset.MinValue;

                        if (DateTimeOffset.UtcNow - lastValidated < TimeSpan.FromMinutes(5))
                        {
                            return;
                        }

                        var userIdClaim = context.Principal?.FindFirst(ClaimTypes.NameIdentifier);
                        if (userIdClaim is null || !int.TryParse(userIdClaim.Value, out var userId))
                        {
                            context.RejectPrincipal();
                            return;
                        }

                        var identityGateway = context.HttpContext.RequestServices.GetRequiredService<IIdentityGateway>();
                        var isActive = await identityGateway.IsUserActiveAsync(userId, context.HttpContext.RequestAborted);

                        if (!isActive)
                        {
                            context.RejectPrincipal();
                            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                            return;
                        }

                        // Re-checked every 5 minutes here (not on every request) — same staleness window already accepted for IsActive above. Revoking admin in the DB takes effect for an already-signed-in session within 5 minutes, not instantly.
                        var isAdmin = await identityGateway.IsUserAdminAsync(userId, context.HttpContext.RequestAborted);

                        var identity = (ClaimsIdentity)context.Principal!.Identity!;
                        var existingLvClaim = identity.FindFirst("lv");
                        if (existingLvClaim is not null)
                        {
                            identity.RemoveClaim(existingLvClaim);
                        }
                        identity.AddClaim(new Claim("lv", DateTimeOffset.UtcNow.ToString("O")));

                        var existingAdminClaim = identity.FindFirst(PortalBffConstants.AdminClaimType);
                        if (existingAdminClaim is not null)
                        {
                            identity.RemoveClaim(existingAdminClaim);
                        }
                        if (isAdmin)
                        {
                            identity.AddClaim(new Claim(PortalBffConstants.AdminClaimType, "true"));
                        }

                        context.ShouldRenew = true;
                    };
                }).AddGoogle(options =>
                {
                    options.ClientId = configuration["Authentication:Google:ClientId"] ?? throw new InvalidOperationException("Authentication:Google:ClientId is not configured.");
                    options.ClientSecret = configuration["Authentication:Google:ClientSecret"] ?? throw new InvalidOperationException("Authentication:Google:ClientSecret is not configured.");

                    options.Scope.Add("email");
                    options.Scope.Add("profile");

                    options.ClaimActions.MapJsonKey("email_verified", "email_verified");

                    options.Events.OnCreatingTicket = async context =>
                    {
                        var identityGateway = context.HttpContext.RequestServices.GetRequiredService<IIdentityGateway>();

                        var providerKey = context.Identity!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
                        var email = context.Identity.FindFirst(ClaimTypes.Email)!.Value;

                        // Google's claim action serializes the JSON boolean via bool.ToString() ("True", not "true"), so a case-sensitive/exact-lowercase comparison here always misses — use bool.TryParse (case-insensitive) instead.
                        var emailVerified = bool.TryParse(context.Identity.FindFirst("email_verified")?.Value, out var verified) && verified;
                        var displayName = context.Identity.FindFirst(ClaimTypes.Name)?.Value;

                        var result = await identityGateway.ProvisionGoogleUserAsync(providerKey, email, emailVerified, displayName, context.HttpContext.RequestAborted);

                        if (!result.Success)
                        {
                            throw new GoogleProvisioningRejectedException(result.RejectionReason ?? "unknown");
                        }

                        var googleIdClaim = context.Identity.FindFirst(ClaimTypes.NameIdentifier);
                        if (googleIdClaim is not null)
                        {
                            context.Identity.RemoveClaim(googleIdClaim);
                        }

                        context.Identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, result.UserId!.Value.ToString()));
                        context.Identity.AddClaim(new Claim(PortalClaimTypes.CultureCode, result.CultureCode!));

                        // Without this, a freshly-signed-in admin would be locked out of /bff/admin/* for up to 5 minutes until OnValidatePrincipal's next periodic refresh adds the claim.
                        if (await identityGateway.IsUserAdminAsync(result.UserId!.Value, context.HttpContext.RequestAborted))
                        {
                            context.Identity.AddClaim(new Claim(PortalBffConstants.AdminClaimType, "true"));
                        }

                        // No explicit SignInAsync exists on this path — the cookie middleware issues it
                        // automatically once this event returns, so this is the only reliable choke
                        // point to warm ICurrentUserConfig's cache for a Google-authenticated user.
                        var configWarmer = context.HttpContext.RequestServices.GetRequiredService<IUserConfigCacheWarmer>();
                        await configWarmer.WarmAsync(result.UserId!.Value, context.HttpContext.RequestAborted);
                        var themeWarmer = context.HttpContext.RequestServices.GetRequiredService<IUserThemeCacheWarmer>();
                        await themeWarmer.WarmAsync(result.UserId!.Value, context.HttpContext.RequestAborted);
                    };

                    options.Events.OnRemoteFailure = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<GoogleProvisioningRejectedException>>();
                        logger.LogWarning(context.Failure, "OnRemoteFailure: Google sign-in did not complete");

                        var reason = context.Failure is GoogleProvisioningRejectedException rejected ? rejected.Reason : "unknown";
                        var frontendBaseUrl = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Frontend:BaseUrl"];

                        context.HandleResponse();
                        context.Response.Redirect($"{frontendBaseUrl}/auth-error?reason={Uri.EscapeDataString(reason)}");
                        return Task.CompletedTask;
                    };
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(PortalBffConstants.AdminOnlyPolicy, policy => policy.RequireClaim(PortalBffConstants.AdminClaimType, "true"));
            });

            return services;
        }
    }
}
