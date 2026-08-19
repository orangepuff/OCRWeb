using OrangepuffPortal.ConfigData.Contract;
using OrangepuffPortal.ConfigData.Contract.Interfaces;

namespace OrangepuffPortal.Bff.Infrastructure.ConfigDataGateway;

/// <summary>
/// In-process <see cref="IConfigDataGateway"/> — forwards straight to <see cref="IConfigDataAdminService"/>.
/// </summary>
public class ConfigDataGateway(IConfigDataAdminService service) : IConfigDataGateway
{
    public Task<IReadOnlyList<ConfigDataDto>> GetAllAsync(CancellationToken ct = default) =>
        service.GetAllAsync(ct);

    public Task<ConfigDataAdminResult> CreateAsync(ConfigDataUpsertRequest request, int actorUserId, CancellationToken ct = default) =>
        service.CreateAsync(request, actorUserId, ct);

    public Task<ConfigDataAdminResult> UpdateAsync(int id, ConfigDataUpsertRequest request, int actorUserId, CancellationToken ct = default) =>
        service.UpdateAsync(id, request, actorUserId, ct);

    public Task<ConfigDataAdminResult> DeleteAsync(int id, CancellationToken ct = default) =>
        service.DeleteAsync(id, ct);
}
