namespace OrangepuffPortal.Theme.Contract.Interfaces;

/// <summary>
/// Resolves and caches the theme for a user immediately after login so <see cref="ICurrentUserTheme"/> can read synchronously.
/// </summary>
public interface IUserThemeCacheWarmer
{
    Task WarmAsync(int userId, CancellationToken cancellationToken = default);
}
