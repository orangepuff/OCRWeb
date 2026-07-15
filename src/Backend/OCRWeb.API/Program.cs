using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using OCRWeb.API.Infrastructure;
using OCRWeb.DocumentProcessing.Infrastructure;
using OCRWeb.Identity.Infrastructure;
using OCRWeb.Identity.Infrastructure.Seeding;
using OCRWeb.Shared.Auditing;

var builder = WebApplication.CreateBuilder(args);

// Web / API surface.
builder.Services.AddOpenApi();
builder.Services.AddFastEndpoints();

// Current user (stub until auth is added) — used to fill audit fields.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

// Modules (each registers its own DbContext, repositories, and MediatR handlers).
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddDocumentProcessing(builder.Configuration);

var app = builder.Build();

// Migrations + seed for every module, gated by a single flag. The database itself is
// assumed to exist; migrations only create/upgrade schema and tables.
if (builder.Configuration.GetValue<bool>("bMigration"))
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    await services.GetRequiredService<UserDbContext>().Database.MigrateAsync();
    await services.GetRequiredService<DocumentProcessingDbContext>().Database.MigrateAsync();
    await services.GetRequiredService<UserDbSeeder>().SeedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseFastEndpoints();

app.Run();
