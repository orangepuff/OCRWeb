using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using OCRWeb.API.Infrastructure;
using OCRWeb.Document.Api;
using OCRWeb.Document.Infrastructure;
using OCRWeb.Identity.Infrastructure;
using OCRWeb.Identity.Infrastructure.Seeding;
using OCRWeb.OCR.Infrastructure;
using OCRWeb.Pdf;
using OCRWeb.Shared.Auditing;

var builder = WebApplication.CreateBuilder(args);

// Web / API surface. Endpoints live in the per-module *.Api assemblies; point discovery at them.
builder.Services.AddOpenApi();
builder.Services.AddFastEndpoints(o => o.Assemblies = [typeof(DocumentApiMarker).Assembly]);

// Current user (stub until auth is added) — used to fill audit fields.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

// Shared PDF engine (technical adapter behind IPdfManipulator, consumed by the modules).
builder.Services.AddPdfEngine();

// Modules (each registers its own DbContext, repositories, and MediatR handlers).
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddDocument(builder.Configuration);
builder.Services.AddOcr(builder.Configuration);

var app = builder.Build();

// Migrations + seed for every module, gated by a single flag. The database itself is
// assumed to exist; migrations only create/upgrade schema and tables.
if (builder.Configuration.GetValue<bool>("bMigration"))
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

app.UseHttpsRedirection();
app.UseFastEndpoints();

app.Run();
