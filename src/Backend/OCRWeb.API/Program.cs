using Diagnostics.Abstractions.Interfaces;
using Diagnostics.AspNetCore.DependencyInjection;
using Diagnostics.NLog.DependencyInjection;
using Diagnostics.NLog.Transactions;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OCRWeb.API.Infrastructure;
using OCRWeb.Document.Api;
using OCRWeb.Document.Infrastructure;
using OCRWeb.Identity.Infrastructure;
using OCRWeb.Identity.Infrastructure.Seeding;
using OCRWeb.OCR.Infrastructure;
using OCRWeb.Pdf;
using OCRWeb.Shared.Auditing;
using System.Reflection;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

// Web / API surface. Endpoints live in the per-module *.Api assemblies; point discovery at them.
builder.Services.AddOpenApi();
builder.Services.AddFastEndpoints(o => o.Assemblies = [typeof(DocumentApiMarker).Assembly, Assembly.GetExecutingAssembly()]);

// Current user (stub until auth is added) — used to fill audit fields.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

// Shared PDF engine (technical adapter behind IPdfManipulator, consumed by the modules).
builder.Services.AddPdfEngine();

// Diagnostics logging (Diagnostics.* — see docs/diagnostics-logging-design.md). Separate
// DiagnosticLogs database/connection string so logging load never contends with OCRWeb itself.
builder.Services.AddDiagnostics(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("DiagnosticLogs")
        ?? throw new InvalidOperationException("ConnectionStrings:DiagnosticLogs is not configured.");
    options.LoggerName = builder.Configuration["Diagnostics:LoggerName"] ?? "OCRWeb.API";
    options.EnvironmentName = builder.Configuration["Diagnostics:EnvironmentName"] ?? "DEV";
});
builder.Services.AddDiagnosticsAspNetCore();

// Auto-stamp every transaction span with the current request's user (overrides the base registration from AddDiagnostics; scoped because ICurrentUser is scoped).
builder.Services.AddScoped<ITransactionLogger>(sp => new RequestContextTransactionLogger(sp.GetRequiredService<TransactionLoggerImplementation>(), sp.GetRequiredService<ICurrentUser>(), sp.GetRequiredService<IHttpContextAccessor>()));

// JWT bearer auth for internal Bff -> API calls only (RS256, verify-only — see
// docs/Authentication/authentication-design.docx §6). Existing endpoints stay open unless
// they explicitly opt into this scheme; this does not gate anything by default.
var publicKeyBase64 = builder.Configuration["Authentication:InternalToken:PublicKey"]
                      ?? throw new InvalidOperationException("Authentication:InternalToken:PublicKey is not configured.");
var rsa = RSA.Create();
rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKeyBase64), out _);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "ocrweb-bff",
            ValidateAudience = true,
            ValidAudience = "ocrweb-api",
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new RsaSecurityKey(rsa),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(10),
        };
    });
builder.Services.AddAuthorization();

// Modules (each registers its own DbContext, repositories, and MediatR handlers).
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddDocument(builder.Configuration);
builder.Services.AddOcr(builder.Configuration);

var app = builder.Build();

// Migrations + seed for every module, gated by a single flag. The database itself is
// assumed to exist; migrations only create/upgrade schema and tables.
if (builder.Configuration.GetValue<bool>("DoMigration"))
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    await services.GetRequiredService<UserDbContext>().Database.MigrateAsync();
    await services.GetRequiredService<DocumentDbContext>().Database.MigrateAsync();
    await services.GetRequiredService<UserDbSeeder>().SeedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// No UseHttpsRedirection(): API is only ever called by OCRWeb.Bff over the internal Docker
// network (plain HTTP, container hostname "api") — the dev HTTPS cert is issued for "localhost"
// only, so forcing a redirect to the HTTPS port here just fails the TLS handshake with a
// hostname mismatch. The external HTTPS port (7101/8081) still exists for anyone who connects
// to it directly; it's just no longer force-redirected to from plain HTTP.

// Authentication must run before UseDiagnostics(): its transaction middleware auto-stamps the
// current user (RequestContextTransactionLogger -> CurrentUser.UserId), which now throws instead
// of falling back to a hardcoded id — HttpContext.User has to already be populated by the time it runs.
app.UseAuthentication();
app.UseAuthorization();

// Correlation id + per-request transaction span (metadata only — see Diagnostics.AspNetCore).
app.UseDiagnostics();

app.UseFastEndpoints();

app.Run();
