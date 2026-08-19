namespace OrangepuffPortal.ConfigData.Contract.Interfaces;

/// <summary>
/// Read-only access to application-level ConfigData, backed by a Redis cache loaded at startup.
/// Intended for any caller app that needs to read named config values without depending on the
/// admin service or the database directly.
/// </summary>
public interface IConfigDataService
{
    /// <summary>
    /// Returns the value for <paramref name="key"/>, or <c>null</c> if no row with that key exists.
    /// </summary>
    Task<string?> GetAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all ConfigData entries from the cache.
    /// </summary>
    Task<IReadOnlyList<ConfigDataDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
