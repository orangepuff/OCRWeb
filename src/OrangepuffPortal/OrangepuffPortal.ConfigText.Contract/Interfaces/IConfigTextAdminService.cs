using OrangepuffPortal.Shared.Paging;

namespace OrangepuffPortal.ConfigText.Contract.Interfaces;

/// <summary>
/// Admin CRUD over [configtext].[ConfigTextDefinition] — distinct from the seed-only <see cref="IConfigTextWriter"/>.
/// </summary>
public interface IConfigTextAdminService
{
    Task<PagedResult<ConfigTextAdminDto>> ListAsync(
        string? module, string? textCode, string? cultureCode, string? textType, string? text, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<ConfigTextAdminResult> AddAsync(ConfigTextAdminUpsertRequest request, int actorUserId, CancellationToken cancellationToken = default);

    Task<ConfigTextAdminResult> UpdateAsync(int id, ConfigTextAdminUpsertRequest request, int actorUserId, CancellationToken cancellationToken = default);

    Task<ConfigTextAdminResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
