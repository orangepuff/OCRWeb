using Diagnostics.AspNetCore.DependencyInjection;
using Diagnostics.NLog.DependencyInjection;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using OCRWeb.Document.Api;
using OCRWeb.Document.Infrastructure;
using OCRWeb.OCR.Infrastructure;
using OCRWeb.Pdf;
using OrangepuffPortal.Host;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Web / API surface. Endpoints live in the per-module *.Api assemblies; point discovery at them.
builder.Services.AddOpenApi();
builder.Services.AddFastEndpoints(o => o.Assemblies = [typeof(DocumentApiMarker).Assembly, Assembly.GetExecutingAssembly()]);

// Shared PDF engine (technical adapter behind IPdfManipulator, consumed by the modules).
builder.Services.AddPdfEngine();

// Diagnostics logging (Diagnostics.* — see docs/diagnostics-logging-design.md). Separate
// DiagnosticLogs database/connection string so logging load never contends with OCRWeb itself.
// Required by OrangepuffPortal.Identity's command handlers (hard ITransactionLogger dependency).
builder.Services.AddDiagnostics(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("DiagnosticLogs")
        ?? throw new InvalidOperationException("ConnectionStrings:DiagnosticLogs is not configured.");
    options.LoggerName = builder.Configuration["Diagnostics:LoggerName"] ?? "OCRWeb.API";
    options.EnvironmentName = builder.Configuration["Diagnostics:EnvironmentName"] ?? "DEV";
});
builder.Services.AddDiagnosticsAspNetCore();

// Identity, cookie + Google OAuth auth, and the /bff/* endpoints — all from the OrangepuffPortal.Host
// package (registers ICurrentUser, auto-stamped transaction logging, and the AdminOnly policy).
builder.Services.AddOrangepuffPortal(builder.Configuration);

// Modules (each registers its own DbContext, repositories, and MediatR handlers).
builder.Services.AddDocument(builder.Configuration);
builder.Services.AddOcr(builder.Configuration);

var app = builder.Build();

// Migrations + seed for every OrangepuffPortal module (Identity), gated by DoMigration.
await app.MigratePortalModulesAsync();

// Document isn't an IPortalModule, so it still migrates manually here.
if (builder.Configuration.GetValue<bool>("DoMigration"))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<DocumentDbContext>().Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// No UseHttpsRedirection(): the frontend always calls the HTTPS port (7201) directly (both
// ng serve's proxy and the hardcoded /bff/login redirect origin), so nothing relies on an
// HTTP->HTTPS redirect here.

// Authentication must run before UseDiagnostics(): its transaction middleware auto-stamps the
// current user (RequestContextTransactionLogger -> CurrentUser.UserId), which throws instead
// of falling back to a hardcoded id — HttpContext.User has to already be populated by the time it runs.
app.UseAuthentication();
app.UseAuthorization();

// Correlation id + per-request transaction span (metadata only — see Diagnostics.AspNetCore).
app.UseDiagnostics();

app.MapOrangepuffPortal();
app.UseFastEndpoints();

app.Run();
