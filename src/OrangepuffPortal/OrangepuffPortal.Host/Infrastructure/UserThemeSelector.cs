using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrangepuffPortal.Identity.Infrastructure;
using OrangepuffPortal.Theme.Contract.Interfaces;
using OrangepuffPortal.Theme.Domain.Repositories;

namespace OrangepuffPortal.Host.Infrastructure;

/// <summary>
/// Coordinates the DB write (Identity's <see cref="UserDbContext"/>) with the Theme cache warmer —
/// the only place in the composition root that needs to span both module contexts.
/// </summary>
internal sealed class UserThemeSelector(
    UserDbContext db,
    IThemeRepository themeRepository,
    IUserThemeCacheWarmer cacheWarmer,
    ILogger<UserThemeSelector> logger) : IUserThemeSelector
{
    private const string LogPrefix = nameof(UserThemeSelector) + "." + nameof(SelectThemeAsync);

    public async Task SelectThemeAsync(int userId, int themeId, CancellationToken cancellationToken = default)
    {
        if (themeId != 0 && !await themeRepository.ExistsAsync(themeId, cancellationToken))
        {
            logger.LogWarning("{LogPrefix}: theme {ThemeId} not found — rejecting selection for user {UserId}", LogPrefix, themeId, userId);
            throw new InvalidOperationException($"Theme {themeId} does not exist.");
        }

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
        {
            logger.LogError("{LogPrefix}: user {UserId} not found", LogPrefix, userId);
            throw new InvalidOperationException($"User {userId} not found.");
        }

        user.SetTheme(themeId, DateTime.UtcNow);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("{LogPrefix}: user {UserId} selected iThemeId={ThemeId}", LogPrefix, userId, themeId);

        await cacheWarmer.WarmAsync(userId, cancellationToken);
    }
}
