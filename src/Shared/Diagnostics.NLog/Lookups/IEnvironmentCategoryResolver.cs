namespace Diagnostics.NLog.Lookups;

/// <summary>
/// Resolves <c>[Environments]</c>/<c>[Categories]</c> names to ids, caching results and inserting rows that don't exist yet (design doc §6: "resolve id by name once, cache, insert-if-missing").
/// </summary>
public interface IEnvironmentCategoryResolver
{
    Task<int> ResolveEnvironmentIdAsync(string name, string? version, string? url, CancellationToken ct = default);

    Task<int> ResolveCategoryIdAsync(string name, CancellationToken ct = default);
}
