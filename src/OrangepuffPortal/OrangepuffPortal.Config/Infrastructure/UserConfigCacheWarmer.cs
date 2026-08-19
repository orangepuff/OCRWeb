using Microsoft.Extensions.Logging;
using OrangepuffPortal.Config.Contract.Interfaces;

namespace OrangepuffPortal.Config.Infrastructure;

/// <summary>
/// Loads a user's config values once and stores them in <see cref="UserConfigCache"/> — called from
/// both Bff login flows (password and Google) right after the auth cookie is issued.
/// </summary>
internal sealed class UserConfigCacheWarmer(
    IConfigUserValueService userValueService,
    UserConfigCache cache,
    ILogger<UserConfigCacheWarmer> logger) : IUserConfigCacheWarmer
{
    public async Task WarmAsync(int userId, CancellationToken cancellationToken = default)
    {
        const string LogPrefix = nameof(UserConfigCacheWarmer) + "." + nameof(WarmAsync);

        var values = await userValueService.GetValuesAsync(userId, cancellationToken);
        await cache.SetAsync(userId, values, cancellationToken);

        logger.LogDebug("{LogPrefix}: warmed {Count} config value(s) for user {UserId}", LogPrefix, values.Count, userId);
    }
}
