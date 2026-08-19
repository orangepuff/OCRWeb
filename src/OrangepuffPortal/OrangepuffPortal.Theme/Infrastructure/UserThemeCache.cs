using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OrangepuffPortal.Theme.Contract;
using System.Text.Json;

namespace OrangepuffPortal.Theme.Infrastructure;

/// <summary>
/// Per-user snapshot cache of the user's resolved <see cref="ThemeDto"/> — populated eagerly at login
/// and read synchronously by <see cref="CurrentUserTheme"/>.
/// Uses the same L1 (IMemoryCache) + L2 (IDistributedCache / Redis) two-layer design as UserConfigCache.
/// Sliding expiration mirrors the auth cookie's own ExpireTimeSpan.
/// </summary>
internal sealed class UserThemeCache(IMemoryCache memoryCache, IDistributedCache distributedCache, ILogger<UserThemeCache> logger)
{
    private static readonly DistributedCacheEntryOptions RedisOptions =
        new() { SlidingExpiration = TimeSpan.FromHours(8) };

    private static readonly MemoryCacheEntryOptions MemoryOptions =
        new() { SlidingExpiration = TimeSpan.FromHours(8) };

    public bool TryGet(int userId, out ThemeDto? theme)
    {
        const string LogPrefix = nameof(UserThemeCache) + "." + nameof(TryGet);

        if (memoryCache.TryGetValue(CacheKey(userId), out theme))
        {
            return true;
        }

        try
        {
            var bytes = distributedCache.Get(CacheKey(userId));
            if (bytes is not null)
            {
                theme = JsonSerializer.Deserialize<ThemeDto>(bytes);
                memoryCache.Set(CacheKey(userId), theme, MemoryOptions);
                return true;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning("{LogPrefix}: L2 cache read failed for user {UserId} — {ExceptionType}: {Message}", LogPrefix, userId, ex.GetType().Name, ex.Message);
        }

        theme = null;
        return false;
    }

    public async Task SetAsync(int userId, ThemeDto theme, CancellationToken cancellationToken = default)
    {
        const string LogPrefix = nameof(UserThemeCache) + "." + nameof(SetAsync);

        memoryCache.Set(CacheKey(userId), theme, MemoryOptions);

        try
        {
            await distributedCache.SetAsync(CacheKey(userId), JsonSerializer.SerializeToUtf8Bytes(theme), RedisOptions, cancellationToken);
            logger.LogInformation("{LogPrefix}: cached theme '{ThemeCode}' for user {UserId}", LogPrefix, theme.ThemeCode, userId);
        }
        catch (Exception ex)
        {
            logger.LogWarning("{LogPrefix}: L2 cache write failed for user {UserId} — {ExceptionType}: {Message}", LogPrefix, userId, ex.GetType().Name, ex.Message);
        }
    }

    public async Task InvalidateAsync(int userId, CancellationToken cancellationToken = default)
    {
        const string LogPrefix = nameof(UserThemeCache) + "." + nameof(InvalidateAsync);

        memoryCache.Remove(CacheKey(userId));

        try
        {
            await distributedCache.RemoveAsync(CacheKey(userId), cancellationToken);
            logger.LogInformation("{LogPrefix}: invalidated theme cache for user {UserId}", LogPrefix, userId);
        }
        catch (Exception ex)
        {
            logger.LogWarning("{LogPrefix}: L2 cache invalidation failed for user {UserId} — {ExceptionType}: {Message}", LogPrefix, userId, ex.GetType().Name, ex.Message);
        }
    }

    private static string CacheKey(int userId) => $"UserTheme:{userId}";
}
