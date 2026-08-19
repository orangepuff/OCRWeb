using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrangepuffPortal.ConfigData.Domain.Repositories;

namespace OrangepuffPortal.ConfigData.Infrastructure;

/// <summary>
/// Eagerly populates <see cref="ConfigDataCache"/> from the DB during application startup so the
/// cache is hot before the first request arrives. Skips gracefully (logs a warning) when the schema
/// is not yet ready — either because migrations are pending or the DB is in an inconsistent state.
/// </summary>
internal sealed class ConfigDataCacheWarmer(
    IServiceScopeFactory scopeFactory,
    ConfigDataCache cache,
    ILogger<ConfigDataCacheWarmer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        const string LogPrefix = nameof(ConfigDataCacheWarmer) + "." + nameof(StartAsync);

        using var scope = scopeFactory.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<ConfigDataDbContext>();
        var pending = await db.Database.GetPendingMigrationsAsync(cancellationToken);
        if (pending.Any())
        {
            logger.LogWarning("{LogPrefix}: skipping warm-up — {Count} pending migration(s) not yet applied", LogPrefix, pending.Count());
            return;
        }

        try
        {
            var repository = scope.ServiceProvider.GetRequiredService<IConfigDataRepository>();
            var all = await repository.GetAllAsync(cancellationToken);
            var rows = all.Select(e => new ConfigDataCacheRow(e.Id, e.Key, e.Value, e.AllowEditByScreen, e.Description)).ToList();
            await cache.SetAllAsync(rows, cancellationToken);

            logger.LogInformation("{LogPrefix}: warmed {Count} ConfigData entries", LogPrefix, rows.Count);
        }
        catch (Exception ex)
        {
            logger.LogWarning("{LogPrefix}: skipping warm-up — {ExceptionType}: {Message}", LogPrefix, ex.GetType().Name, ex.Message);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
