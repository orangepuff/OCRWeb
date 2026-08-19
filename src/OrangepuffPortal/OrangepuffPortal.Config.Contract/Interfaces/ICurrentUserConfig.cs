namespace OrangepuffPortal.Config.Contract.Interfaces;

/// <summary>
/// Synchronous read of the signed-in user's config values, backed by a cache warmed once at login
/// (see <see cref="IUserConfigCacheWarmer"/>) rather than loaded lazily here — a hot-path check like an
/// upload-size limit never awaits a database round trip.
/// Falls back to the caller-supplied default if the user has no value for a config, or if the cache
/// was never warmed for this session (e.g. it expired mid-session).
/// </summary>
public interface ICurrentUserConfig
{
    int GetInt(string configCode, int fallback = default);

    decimal GetDecimal(string configCode, decimal fallback = default);

    bool GetBool(string configCode, bool fallback = default);

    string? GetString(string configCode, string? fallback = null);

    /// <summary>All of the current user's cached config values, e.g. for building a settings summary.</summary>
    IReadOnlyList<ConfigUserValueDto> GetAll();
}
