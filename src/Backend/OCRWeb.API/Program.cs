using Diagnostics.AspNetCore.DependencyInjection;
using Diagnostics.NLog.DependencyInjection;
using FastEndpoints;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using OCRWeb.API.ConfigText;
using OCRWeb.Document.Api;
using OCRWeb.Document.Infrastructure;
using OCRWeb.OCR.Infrastructure;
using OCRWeb.Pdf;
using OCRWeb.ProjectManagement.Api;
using OCRWeb.ProjectManagement.Infrastructure;
using OrangepuffPortal.ConfigText.Contract.Interfaces;
using OrangepuffPortal.Host;
using System.Reflection;

// Allow DateTime.UtcNow to be written to PostgreSQL timestamp columns (Npgsql 6+ default requires timestamptz).
// The entire app stores UTC; this avoids changing every column type annotation.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

const long MaxUploadBytes = 1024L * 1024 * 1024; // 1 GB — covers large scanned PDF documents.

var builder = WebApplication.CreateBuilder(args);

// Kestrel's default MaxRequestBodySize (~28.6 MB) silently rejects anything bigger, which is far
// too small for scanned PDF uploads. FormOptions.MultipartBodyLengthLimit gates IFormFile binding
// specifically and needs raising too (its own default is 128 MB).
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = MaxUploadBytes);
builder.Services.Configure<FormOptions>(options => options.MultipartBodyLengthLimit = MaxUploadBytes);

// Web / API surface. Endpoints live in the per-module *.Api assemblies; point discovery at them.
builder.Services.AddOpenApi();
builder.Services.AddFastEndpoints(o => o.Assemblies =
    [typeof(DocumentApiMarker).Assembly, typeof(ProjectManagementApiMarker).Assembly, Assembly.GetExecutingAssembly()]);

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
builder.Services.AddProjectManagement(builder.Configuration);

var app = builder.Build();

// Migrations + seed for every OrangepuffPortal module (Identity), gated by DoMigration.
await app.MigratePortalModulesAsync();

// Document/ProjectManagement aren't IPortalModules, so they still migrate manually here.
if (builder.Configuration.GetValue<bool>("DoMigration"))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<DocumentDbContext>().Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<ProjectDbContext>().Database.MigrateAsync();

    // Push every module's own default label/message text into the shared ConfigTextDefinition
    // table (owned by orangepuffportal). In-process call, insert-only unless a seed entry sets
    // btReplace — see docs/config-text-consumption.md.
    var configTextWriter = scope.ServiceProvider.GetRequiredService<IConfigTextWriter>();
    await configTextWriter.UpsertManyAsync("en-US", ConfigTextSeed.LoadAll("en-US"));
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
