using OrangepuffPortal.ConfigText.Contract;
using OrangepuffPortal.Shared.Paging;

namespace OrangepuffPortal.Bff.Infrastructure.ConfigTextGateway
{
    /// <summary>
    /// Facade the Bff uses to reach the ConfigText module's admin service, in-process.
    /// </summary>
    public interface IConfigTextGateway
    {
        Task<PagedResult<ConfigTextAdminDto>> ListAsync(
            string? module, string? textCode, string? cultureCode, string? textType, string? text, int page, int pageSize, CancellationToken ct = default);

        Task<ConfigTextAdminResult> AddAsync(ConfigTextAdminUpsertRequest request, int actorUserId, CancellationToken ct = default);

        Task<ConfigTextAdminResult> UpdateAsync(int id, ConfigTextAdminUpsertRequest request, int actorUserId, CancellationToken ct = default);

        Task<ConfigTextAdminResult> DeleteAsync(int id, CancellationToken ct = default);
    }
}
