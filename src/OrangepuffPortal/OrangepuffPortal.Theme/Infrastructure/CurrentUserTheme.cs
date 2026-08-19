using Microsoft.Extensions.Logging;
using OrangepuffPortal.Theme.Contract;
using OrangepuffPortal.Theme.Contract.Interfaces;
using OrangepuffPortal.Shared.Auditing;

namespace OrangepuffPortal.Theme.Infrastructure;

/// <summary>
/// Reads the current user's resolved theme out of <see cref="UserThemeCache"/> — never the database
/// directly. The cache is expected to already be warm (see <see cref="UserThemeCacheWarmer"/>).
/// </summary>
internal sealed class CurrentUserTheme(
    ICurrentUser currentUser,
    UserThemeCache cache,
    ILogger<CurrentUserTheme> logger) : ICurrentUserTheme
{
    public ThemeDto GetTheme()
    {
        const string LogPrefix = nameof(CurrentUserTheme) + "." + nameof(GetTheme);

        if (cache.TryGet(currentUser.UserId, out var theme) && theme is not null)
        {
            return theme;
        }

        logger.LogWarning("{LogPrefix}: no cached theme for user {UserId}; returning empty theme shell", LogPrefix, currentUser.UserId);
        return new ThemeDto { ThemeCode = Domain.Entity.Theme.DefaultThemeCode };
    }
}
