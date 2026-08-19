using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using OrangepuffPortal.ConfigText.Domain.Entity;
using System.Collections.Concurrent;
using System.Text.Json;

namespace OrangepuffPortal.ConfigText.Infrastructure;

/// <summary>
/// Thin serializable projection of <see cref="ConfigTextDefinition"/> used for Redis serialization — holds only the fields that <see cref="ConfigTextReader"/> needs after a cache hit.
/// </summary>
internal record ConfigTextCacheRow(string Module, string TextCode, string CultureCode, string TextType, string Text);

/// <summary>
/// Caches resolved rows per requested culture in <see cref="IDistributedCache"/> (Redis when
/// <c>ConnectionStrings:Redis</c> is set, otherwise an in-process fallback). A single write can
/// change the "*" fallback row consulted by every culture, so <see cref="InvalidateAsync"/> evicts
/// every cached culture at once. Redis failures are treated as cache misses and logged as warnings.
/// </summary>
/// <remarks>
/// Culture codes that have been loaded are tracked in a per-instance <see cref="ConcurrentDictionary"/>
/// so invalidation can remove those exact keys from Redis. In a multi-instance deployment, an instance
/// that never served a read for a given culture will not clear that culture's Redis key on invalidate;
/// the 24-hour absolute expiration acts as a safety-net TTL in that scenario.
/// </remarks>
internal sealed class ConfigTextCache(IDistributedCache cache, ILogger<ConfigTextCache> logger)
{
    private static readonly DistributedCacheEntryOptions EntryOptions = new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) };

    private readonly ConcurrentDictionary<string, bool> _loadedCultures = new(StringComparer.OrdinalIgnoreCase);

    public async Task<IReadOnlyList<ConfigTextCacheRow>> GetOrCreateAsync(
        string cultureCode,
        Func<Task<IReadOnlyList<ConfigTextCacheRow>>> factory,
        CancellationToken cancellationToken = default)
    {
        const string LogPrefix = nameof(ConfigTextCache) + "." + nameof(GetOrCreateAsync);
        logger.LogInformation("{LogPrefix}: resolving cache for culture '{Culture}'", LogPrefix, cultureCode);

        try
        {
            var bytes = await cache.GetAsync(CacheKey(cultureCode), cancellationToken);
            if (bytes is not null)
            {
                logger.LogInformation("{LogPrefix}: cache hit for culture '{Culture}'", LogPrefix, cultureCode);
                return JsonSerializer.Deserialize<List<ConfigTextCacheRow>>(bytes)!;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning("{LogPrefix}: cache read failed for culture '{Culture}' — {ExceptionType}: {Message}", LogPrefix, cultureCode, ex.GetType().Name, ex.Message);
        }

        logger.LogInformation("{LogPrefix}: cache miss for culture '{Culture}', fetching from DB", LogPrefix, cultureCode);
        var rows = await factory();

        try
        {
            await cache.SetAsync(CacheKey(cultureCode), JsonSerializer.SerializeToUtf8Bytes(rows), EntryOptions, cancellationToken);
            _loadedCultures.TryAdd(cultureCode, true);
            logger.LogInformation("{LogPrefix}: cached {Count} entries for culture '{Culture}'", LogPrefix, rows.Count, cultureCode);
        }
        catch (Exception ex)
        {
            logger.LogWarning("{LogPrefix}: cache write failed for culture '{Culture}' — {ExceptionType}: {Message}", LogPrefix, cultureCode, ex.GetType().Name, ex.Message);
        }

        return rows;
    }

    public async Task InvalidateAsync(CancellationToken cancellationToken = default)
    {
        const string LogPrefix = nameof(ConfigTextCache) + "." + nameof(InvalidateAsync);
        logger.LogInformation("{LogPrefix}: invalidating all culture caches", LogPrefix);

        var cultures = _loadedCultures.Keys.ToArray();
        _loadedCultures.Clear();

        var tasks = cultures
            .Select(c => RemoveSafeAsync(LogPrefix, CacheKey(c), cancellationToken))
            .Append(RemoveSafeAsync(LogPrefix, CacheKey(ConfigTextDefinition.WildcardCulture), cancellationToken));

        await Task.WhenAll(tasks);
        logger.LogInformation("{LogPrefix}: invalidated {Count} culture cache(s)", LogPrefix, cultures.Length);
    }

    private async Task RemoveSafeAsync(string logPrefix, string key, CancellationToken cancellationToken)
    {
        try
        {
            await cache.RemoveAsync(key, cancellationToken);
            logger.LogInformation("{LogPrefix}: removed cache key '{Key}'", logPrefix, key);
        }
        catch (Exception ex)
        {
            logger.LogWarning("{LogPrefix}: cache invalidation failed for key '{Key}' — {ExceptionType}: {Message}", logPrefix, key, ex.GetType().Name, ex.Message);
        }
    }

    private static string CacheKey(string cultureCode) => $"ConfigText:{cultureCode}";
}
