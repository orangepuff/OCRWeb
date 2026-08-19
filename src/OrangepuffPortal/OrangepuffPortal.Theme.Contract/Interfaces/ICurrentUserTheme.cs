namespace OrangepuffPortal.Theme.Contract.Interfaces;

/// <summary>
/// Synchronous read of the signed-in user's resolved theme, backed by a cache warmed once at login.
/// Falls back to the Default theme if the user has no selection or the selected theme no longer exists.
/// </summary>
public interface ICurrentUserTheme
{
    /// <summary>The fully resolved theme for the current user (never null — falls back to Default).</summary>
    ThemeDto GetTheme();
}
