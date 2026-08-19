using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrangepuffPortal.Identity.Infrastructure;
using OrangepuffPortal.Shared.Auditing;

namespace OrangepuffPortal.Host.Infrastructure;

/// <summary>
/// Reads user ids straight from Identity's <see cref="UserDbContext"/> — only the composition root
/// is allowed to reference another module's DbContext directly, same as <see cref="CurrentUser"/>
/// referencing Identity's domain entity directly for its own culture fallback.
/// </summary>
public class UserDirectory(UserDbContext db, ILogger<UserDirectory> logger) : IUserDirectory
{
    private const string LogPrefix = nameof(UserDirectory) + "." + nameof(GetAllUserIdsAsync);

    public async Task<IReadOnlyList<int>> GetAllUserIdsAsync(CancellationToken cancellationToken = default)
    {
        var userIds = await db.Users.AsNoTracking().Select(u => u.Id).ToListAsync(cancellationToken);
        logger.LogDebug("{LogPrefix}: read {Count} user id(s)", LogPrefix, userIds.Count);
        return userIds;
    }

    public async Task<int> GetUserThemeIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        const string LogPrefix = nameof(UserDirectory) + "." + nameof(GetUserThemeIdAsync);
        var themeId = await db.Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.ThemeId)
            .FirstOrDefaultAsync(cancellationToken);
        logger.LogDebug("{LogPrefix}: user {UserId} has iThemeId={ThemeId}", LogPrefix, userId, themeId);
        return themeId;
    }
}
