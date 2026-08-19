using OrangepuffPortal.ConfigText.Domain.Entity;
using OrangepuffPortal.Shared.Paging;

namespace OrangepuffPortal.ConfigText.Domain.Repositories;

public interface IConfigTextRepository
{
    Task<ConfigTextDefinition?> FindAsync(string module, string textCode, string cultureCode, string textType, CancellationToken cancellationToken = default);

    Task<ConfigTextDefinition?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rows at <paramref name="cultureCode"/> plus every wildcard row, for fallback resolution.
    /// </summary>
    Task<IReadOnlyList<ConfigTextDefinition>> GetForCultureAsync(string cultureCode, CancellationToken cancellationToken = default);

    /// <summary>Admin grid listing — every filter is optional (null/blank means "don't filter on this field").</summary>
    Task<PagedResult<ConfigTextDefinition>> ListAsync(
        string? module, string? textCode, string? cultureCode, string? textType, string? textContains, int skip, int take, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string module, string textCode, string cultureCode, string textType, int? excludeId, CancellationToken cancellationToken = default);

    Task AddAsync(ConfigTextDefinition entry, CancellationToken cancellationToken = default);

    Task DeleteAsync(ConfigTextDefinition entry, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
