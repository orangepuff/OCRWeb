namespace OrangepuffPortal.ConfigText.Contract.Interfaces;

/// <summary>
/// Read path for the shared ConfigTextDefinition table, with culture fallback already resolved.
/// </summary>
public interface IConfigTextReader
{
    /// <summary>
    /// Returns one resolved row per (SModule, STextCode, STextType): the row matching
    /// <paramref name="cultureCode"/> if one exists, otherwise the "*" fallback row.
    /// </summary>
    /// <param name="modules">
    /// When non-null and non-empty, only rows whose SModule is in this set are returned — lets a
    /// consuming app fetch just the modules its own frontend actually uses instead of every module
    /// ever seeded by every app sharing this table (see docs/config-text-design.md). Null/empty
    /// returns every module, matching the original unfiltered behavior.
    /// </param>
    Task<IReadOnlyList<ConfigTextEntryDto>> GetAllAsync(
        string cultureCode,
        IReadOnlyCollection<string>? modules = null,
        CancellationToken cancellationToken = default);
}
