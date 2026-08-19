using OrangepuffPortal.ConfigData.Contract;

namespace OrangepuffPortal.Bff.Infrastructure.ConfigDataGateway;

/// <summary>
/// Facade the Bff uses to reach the ConfigData module's admin service, in-process.
/// </summary>
public interface IConfigDataGateway
{
    Task<IReadOnlyList<ConfigDataDto>> GetAllAsync(CancellationToken ct = default);

    Task<ConfigDataAdminResult> CreateAsync(ConfigDataUpsertRequest request, int actorUserId, CancellationToken ct = default);

    Task<ConfigDataAdminResult> UpdateAsync(int id, ConfigDataUpsertRequest request, int actorUserId, CancellationToken ct = default);

    Task<ConfigDataAdminResult> DeleteAsync(int id, CancellationToken ct = default);
}
