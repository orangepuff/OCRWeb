namespace OrangepuffPortal.Theme.Contract.Interfaces;

/// <summary>
/// Changes the theme a user has selected, persists it to the database, and re-warms their cache.
/// </summary>
public interface IUserThemeSelector
{
    /// <summary>
    /// Sets <paramref name="userId"/>'s preferred theme to <paramref name="themeId"/>.
    /// Pass <c>0</c> to revert to the Default theme.
    /// </summary>
    Task SelectThemeAsync(int userId, int themeId, CancellationToken cancellationToken = default);
}
