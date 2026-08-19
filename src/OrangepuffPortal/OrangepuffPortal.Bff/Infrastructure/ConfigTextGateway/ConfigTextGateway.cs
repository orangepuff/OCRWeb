using OrangepuffPortal.ConfigText.Contract;
using OrangepuffPortal.ConfigText.Contract.Interfaces;
using OrangepuffPortal.Shared.Paging;

namespace OrangepuffPortal.Bff.Infrastructure.ConfigTextGateway
{
    /// <summary>
    /// In-process <see cref="IConfigTextGateway"/> — the ConfigText module has no MediatR handlers (unlike
    /// Identity), so this just forwards straight to <see cref="IConfigTextAdminService"/>.
    /// </summary>
    public class ConfigTextGateway(IConfigTextAdminService service) : IConfigTextGateway
    {
        public Task<PagedResult<ConfigTextAdminDto>> ListAsync(
            string? module, string? textCode, string? cultureCode, string? textType, string? text, int page, int pageSize, CancellationToken ct = default) =>
            service.ListAsync(module, textCode, cultureCode, textType, text, page, pageSize, ct);

        public Task<ConfigTextAdminResult> AddAsync(ConfigTextAdminUpsertRequest request, int actorUserId, CancellationToken ct = default) =>
            service.AddAsync(request, actorUserId, ct);

        public Task<ConfigTextAdminResult> UpdateAsync(int id, ConfigTextAdminUpsertRequest request, int actorUserId, CancellationToken ct = default) =>
            service.UpdateAsync(id, request, actorUserId, ct);

        public Task<ConfigTextAdminResult> DeleteAsync(int id, CancellationToken ct = default) =>
            service.DeleteAsync(id, ct);
    }
}
