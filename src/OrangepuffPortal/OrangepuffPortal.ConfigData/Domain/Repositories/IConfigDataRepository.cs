using OrangepuffPortal.ConfigData.Domain.Entity;

namespace OrangepuffPortal.ConfigData.Domain.Repositories;

public interface IConfigDataRepository
{
    Task<IReadOnlyList<ConfigDataEntry>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ConfigDataEntry?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns <c>true</c> if a row with <paramref name="key"/> already exists, optionally
    /// excluding <paramref name="excludeId"/> (for update duplicate-key checks).
    /// </summary>
    Task<bool> ExistsAsync(string key, int? excludeId, CancellationToken cancellationToken = default);

    Task AddAsync(ConfigDataEntry entry, CancellationToken cancellationToken = default);

    Task DeleteAsync(ConfigDataEntry entry, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
