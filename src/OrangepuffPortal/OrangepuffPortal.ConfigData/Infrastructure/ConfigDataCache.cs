using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace OrangepuffPortal.ConfigData.Infrastructure;

/// <summary>
/// Thin serializable projection of <see cref="Domain.Entity.ConfigData"/> used for Redis serialization.
/// </summary>
internal record ConfigDataCacheRow(int Id, string Key, string? Value, bool? AllowEditByScreen, string? Description);

/// <summary>
/// Caches all ConfigData rows as a single <c>ConfigData:all</c> key in <see cref="IDistributedCache"/>
/// (Redis when configured, otherwise an in-process fallback). The entire key is invalidated on any
/// admin write and re-populated on the next read. A 24-hour absolute TTL acts as a safety net.
/// Redis failures are treated as cache misses and logged as warnings — the app continues without cache.
/// </summary>
internal sealed class ConfigDataCache(IDistributedCache distributedCache, ILogger<ConfigDataCache> logger)
{
    internal const string CacheKey = "ConfigData:all";

    private static readonly DistributedCacheEntryOptions EntryOptions =
        new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) };

    public async Task<IReadOnlyList<ConfigDataCacheRow>?> GetAsync(CancellationToken cancellationToken = default)
    {
        const string LogPrefix = nameof(ConfigDataCache) + "." + nameof(GetAsync);
        try
        {
            var bytes = await distributedCache.GetAsync(CacheKey, cancellationToken);
            return bytes is null ? null : JsonSerializer.Deserialize<List<ConfigDataCacheRow>>(bytes);
        }
        catch (Exception ex)
        {
            logger.LogWarning("{LogPrefix}: cache read failed — {ExceptionType}: {Message}", LogPrefix, ex.GetType().Name, ex.Message);
            return null;
        }
    }

    public async Task SetAllAsync(IReadOnlyList<ConfigDataCacheRow> rows, CancellationToken cancellationToken = default)
    {
        const string LogPrefix = nameof(ConfigDataCache) + "." + nameof(SetAllAsync);
        try
        {
            await distributedCache.SetAsync(CacheKey, JsonSerializer.SerializeToUtf8Bytes(rows), EntryOptions, cancellationToken);
            logger.LogInformation("{LogPrefix}: cached {Count} entries", LogPrefix, rows.Count);
        }
        catch (Exception ex)
        {
            logger.LogWarning("{LogPrefix}: cache write failed — {ExceptionType}: {Message}", LogPrefix, ex.GetType().Name, ex.Message);
        }
    }

    public async Task InvalidateAsync(CancellationToken cancellationToken = default)
    {
        const string LogPrefix = nameof(ConfigDataCache) + "." + nameof(InvalidateAsync);
        try
        {
            await distributedCache.RemoveAsync(CacheKey, cancellationToken);
            logger.LogInformation("{LogPrefix}: cache invalidated", LogPrefix);
        }
        catch (Exception ex)
        {
            logger.LogWarning("{LogPrefix}: cache invalidation failed — {ExceptionType}: {Message}", LogPrefix, ex.GetType().Name, ex.Message);
        }
    }
}
