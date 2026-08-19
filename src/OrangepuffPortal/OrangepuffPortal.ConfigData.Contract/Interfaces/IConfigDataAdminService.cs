namespace OrangepuffPortal.ConfigData.Contract.Interfaces;

/// <summary>
/// Admin CRUD over the ConfigData table, called from the Bff admin group.
/// Every mutation invalidates the Redis cache so subsequent reads reflect the change.
/// </summary>
public interface IConfigDataAdminService
{
    Task<IReadOnlyList<ConfigDataDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ConfigDataAdminResult> CreateAsync(ConfigDataUpsertRequest request, int actorUserId, CancellationToken cancellationToken = default);

    Task<ConfigDataAdminResult> UpdateAsync(int id, ConfigDataUpsertRequest request, int actorUserId, CancellationToken cancellationToken = default);

    Task<ConfigDataAdminResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
