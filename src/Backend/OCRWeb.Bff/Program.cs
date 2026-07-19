using Diagnostics.AspNetCore.DependencyInjection;
using Diagnostics.NLog.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using OCRWeb.Bff.Endpoints;
using OCRWeb.Bff.Infrastructure;
using OCRWeb.Bff.Infrastructure.IdentityApiClient;
using OCRWeb.Bff.Infrastructure.InternalToken;
using System.Security.Claims;

const string AdminClaimType = "admin";
const string AdminOnlyPolicy = "AdminOnly";

var builder = WebApplication.CreateBuilder(args);

// Diagnostics logging (Diagnostics.* — see the DiagnosticLog repo's README). Same DiagnosticLogs
// database and LoggerName ("NLogLogger") as OCRWeb.API, so both share one set of DB-configured
// NLog rules; give this a distinct LoggerName instead if it ever needs its own rule tuning.
builder.Services.AddDiagnostics(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("DiagnosticLogs")
        ?? throw new InvalidOperationException("ConnectionStrings:DiagnosticLogs is not configured.");
    options.LoggerName = builder.Configuration["Diagnostics:LoggerName"] ?? "NLogLogger";
    options.EnvironmentName = builder.Configuration["Diagnostics:EnvironmentName"] ?? "DEV";
});
builder.Services.AddDiagnosticsAspNetCore();

builder.Services.AddSingleton<IInternalTokenIssuer, InternalTokenIssuer>();
builder.Services.AddTransient<InternalTokenDelegatingHandler>();

// Persisted to a mounted volume (see docker-compose.yml) so keys survive container
// recreation — without this, every rebuild/restart silently invalidates every signed-in
// user's auth cookie (it can no longer be decrypted), forcing a fresh login.
builder.Services.AddDataProtection()
    .SetApplicationName("OCRWeb.Bff")
    .PersistKeysToFileSystem(new DirectoryInfo("/keys"));

builder.Services.AddHttpClient<IIdentityApiClient, IdentityApiClient>(client =>
    {
        var apiBaseUrl = builder.Configuration["Services:OCRWebApi:BaseUrl"]
                         ?? throw new InvalidOperationException("Services:OCRWebApi:BaseUrl is not configured.");
        client.BaseAddress = new Uri(apiBaseUrl);
    })
    .AddHttpMessageHandler<InternalTokenDelegatingHandler>()
    // Forwards this request's X-Correlation-ID to OCRWeb.API so a single trace spans both
    // services — see the DiagnosticLog repo's README, "Propagate correlation ids".
    .AddDiagnosticsCorrelationPropagation();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.Cookie.Name = ".OCRWeb.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);

        // /bff/me and /bff/logout are JSON-style endpoints — return plain status codes,
        // never redirect to a login page (the cookie handler's default challenge behavior).
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

            var identityApiClient = context.HttpContext.RequestServices.GetRequiredService<IIdentityApiClient>();
            var isActive = await identityApiClient.IsUserActiveAsync(userId, context.HttpContext.RequestAborted);

            if (!isActive)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return;
            }

            // Re-checked every 5 minutes here (not on every request) — same staleness window
            // already accepted for IsActive above. Revoking btAdmin in the DB takes effect for
            // an already-signed-in session within 5 minutes, not instantly.
            var isAdmin = await identityApiClient.IsUserAdminAsync(userId, context.HttpContext.RequestAborted);

            var identity = (ClaimsIdentity)context.Principal!.Identity!;
            var existingLvClaim = identity.FindFirst("lv");
            if (existingLvClaim is not null)
            {
                identity.RemoveClaim(existingLvClaim);
            }
            identity.AddClaim(new Claim("lv", DateTimeOffset.UtcNow.ToString("O")));

            var existingAdminClaim = identity.FindFirst(AdminClaimType);
            if (existingAdminClaim is not null)
            {
                identity.RemoveClaim(existingAdminClaim);
            }
            if (isAdmin)
            {
                identity.AddClaim(new Claim(AdminClaimType, "true"));
            }

            context.ShouldRenew = true;
        };
    }).AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? throw new InvalidOperationException("Authentication:Google:ClientId is not configured.");
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? throw new InvalidOperationException("Authentication:Google:ClientSecret is not configured.");

        options.Scope.Add("email");
        options.Scope.Add("profile");

        options.ClaimActions.MapJsonKey("email_verified", "email_verified");

        options.Events.OnCreatingTicket = async context =>
        {
            var identityApiClient = context.HttpContext.RequestServices.GetRequiredService<IIdentityApiClient>();

            var providerKey = context.Identity!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var email = context.Identity.FindFirst(ClaimTypes.Email)!.Value;
            // Google's claim action serializes the JSON boolean via bool.ToString() ("True", not
            // "true"), so a case-sensitive/exact-lowercase comparison here always misses — use
            // bool.TryParse (case-insensitive) instead.
            var emailVerified = bool.TryParse(context.Identity.FindFirst("email_verified")?.Value, out var verified) && verified;
            var displayName = context.Identity.FindFirst(ClaimTypes.Name)?.Value;

            var result = await identityApiClient.ProvisionGoogleUserAsync(providerKey, email, emailVerified, displayName, context.HttpContext.RequestAborted);

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

            // Without this, a freshly-signed-in admin would be locked out of /bff/admin/* for
            // up to 5 minutes until OnValidatePrincipal's next periodic refresh adds the claim.
            if (await identityApiClient.IsUserAdminAsync(result.UserId!.Value, context.HttpContext.RequestAborted))
            {
                context.Identity.AddClaim(new Claim(AdminClaimType, "true"));
            }
        };

        options.Events.OnRemoteFailure = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning(context.Failure, "OnRemoteFailure: Google sign-in did not complete");

            var reason = context.Failure is GoogleProvisioningRejectedException rejected ? rejected.Reason : "unknown";
            var frontendBaseUrl = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Frontend:BaseUrl"];

            context.HandleResponse();
            context.Response.Redirect($"{frontendBaseUrl}/auth-error?reason={Uri.EscapeDataString(reason)}");
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AdminOnlyPolicy, policy => policy.RequireClaim(AdminClaimType, "true"));
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Authentication must run before UseDiagnostics(): its transaction middleware reads
// HttpContext.User to stamp the signed-in user onto the request's transaction span, so
// HttpContext.User has to already be populated by the time it runs.
app.UseDiagnostics();

app.MapAuthEndpoints();
app.MapAvatarEndpoints();

var adminGroup = app.MapGroup("/bff/admin").RequireAuthorization(AdminOnlyPolicy);
adminGroup.MapUserAdminEndpoints();
adminGroup.MapSecurityRuleCategoryAdminEndpoints();
adminGroup.MapSecurityRuleItemAdminEndpoints();

app.Run();
