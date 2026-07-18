using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using OCRWeb.Bff.Endpoints;
using OCRWeb.Bff.Infrastructure;
using OCRWeb.Bff.Infrastructure.IdentityApiClient;
using OCRWeb.Bff.Infrastructure.InternalToken;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IInternalTokenIssuer, InternalTokenIssuer>();
builder.Services.AddTransient<InternalTokenDelegatingHandler>();

builder.Services.AddHttpClient<IIdentityApiClient, IdentityApiClient>(client =>
    {
        var apiBaseUrl = builder.Configuration["Services:OCRWebApi:BaseUrl"]
                         ?? throw new InvalidOperationException("Services:OCRWebApi:BaseUrl is not configured.");
        client.BaseAddress = new Uri(apiBaseUrl);
    })
    .AddHttpMessageHandler<InternalTokenDelegatingHandler>();

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

            var identity = (ClaimsIdentity)context.Principal!.Identity!;
            var existingLvClaim = identity.FindFirst("lv");
            if (existingLvClaim is not null)
            {
                identity.RemoveClaim(existingLvClaim);
            }
            identity.AddClaim(new Claim("lv", DateTimeOffset.UtcNow.ToString("O")));

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

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();

app.Run();
