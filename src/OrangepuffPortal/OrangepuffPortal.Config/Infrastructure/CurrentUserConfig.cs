using Microsoft.Extensions.Logging;
using OrangepuffPortal.Config.Contract;
using OrangepuffPortal.Config.Contract.Interfaces;
using OrangepuffPortal.Shared.Auditing;

namespace OrangepuffPortal.Config.Infrastructure;

/// <summary>
/// Reads the current signed-in user's config values out of <see cref="UserConfigCache"/>, never the
/// database directly — the cache is expected to already be warm (see <see cref="UserConfigCacheWarmer"/>),
/// which is what makes every method here safely synchronous.
/// </summary>
internal sealed class CurrentUserConfig(
    ICurrentUser currentUser,
    UserConfigCache cache,
    ILogger<CurrentUserConfig> logger) : ICurrentUserConfig
{
    public int GetInt(string configCode, int fallback = default) => Find(configCode)?.IConfigValue ?? Fallback(configCode, fallback);

    public decimal GetDecimal(string configCode, decimal fallback = default) => Find(configCode)?.NConfigValue ?? Fallback(configCode, fallback);

    public bool GetBool(string configCode, bool fallback = default) => Find(configCode)?.BtConfigValue ?? Fallback(configCode, fallback);

    public string? GetString(string configCode, string? fallback = null) => Find(configCode)?.SConfigValue ?? fallback;

    public IReadOnlyList<ConfigUserValueDto> GetAll() => Snapshot();

    private ConfigUserValueDto? Find(string configCode) => Snapshot().FirstOrDefault(value => value.SConfigCode == configCode);

    private IReadOnlyList<ConfigUserValueDto> Snapshot()
    {
        const string LogPrefix = nameof(CurrentUserConfig) + "." + nameof(Snapshot);

        if (cache.TryGet(currentUser.UserId, out var values))
        {
            return values;
        }

        // Only reachable if the cache entry expired/was evicted mid-session, or the session predates
        // this feature — the eager warm at login is the primary path. Callers fall back to their own
        // caller-supplied default until the user's next login re-warms the cache.
        logger.LogWarning("{LogPrefix}: no cached config for user {UserId}; callers will get fallback values", LogPrefix, currentUser.UserId);
        return [];
    }

    private T Fallback<T>(string configCode, T fallback)
    {
        const string LogPrefix = nameof(CurrentUserConfig) + "." + nameof(Fallback);
        logger.LogDebug("{LogPrefix}: no cached value for config '{ConfigCode}' and user {UserId}, using fallback", LogPrefix, configCode, currentUser.UserId);
        return fallback;
    }
}
