using OrangepuffPortal.Config.Contract;
using OrangepuffPortal.Config.Contract.Interfaces;
using OrangepuffPortal.Shared.Paging;

namespace OrangepuffPortal.Bff.Infrastructure.ConfigGateway
{
    /// <summary>
    /// In-process <see cref="IConfigGateway"/> — the Config module has no MediatR handlers (unlike
    /// Identity), so this just forwards straight to <see cref="IConfigUserValueService"/>/<see cref="IConfigCatalogAdminService"/>.
    /// </summary>
    public class ConfigGateway(IConfigUserValueService service, IConfigCatalogAdminService catalogAdminService) : IConfigGateway
    {
        public Task<IReadOnlyList<UserConfigSectionDto>> GetSectionsForUserAsync(int userId, CancellationToken ct = default) =>
            service.GetSectionsForUserAsync(userId, ct);

        public Task SetConfigValueAsync(int userId, string configCode, ConfigValueInput value, CancellationToken ct = default) =>
            service.SetValueAsync(userId, configCode, value, ct);

        public Task<IReadOnlyList<ConfigSectionAdminDto>> ListSectionsAsync(CancellationToken ct = default) =>
            catalogAdminService.ListSectionsAsync(ct);

        public Task<ConfigCatalogAdminResult> AddSectionAsync(ConfigSectionUpsertRequest request, int actorUserId, CancellationToken ct = default) =>
            catalogAdminService.AddSectionAsync(request, actorUserId, ct);

        public Task<ConfigCatalogAdminResult> UpdateSectionAsync(int id, ConfigSectionUpsertRequest request, int actorUserId, CancellationToken ct = default) =>
            catalogAdminService.UpdateSectionAsync(id, request, actorUserId, ct);

        public Task<ConfigCatalogAdminResult> DeleteSectionAsync(int id, CancellationToken ct = default) =>
            catalogAdminService.DeleteSectionAsync(id, ct);

        public Task<PagedResult<ConfigItemAdminDto>> ListConfigsAsync(
            int? sectionId, string? configCode, string? configName, int? configType, string? sortBy, bool sortDescending, int page, int pageSize, CancellationToken ct = default) =>
            catalogAdminService.ListConfigsAsync(sectionId, configCode, configName, configType, sortBy, sortDescending, page, pageSize, ct);

        public Task<ConfigCatalogAdminResult> AddConfigAsync(ConfigItemUpsertRequest request, int actorUserId, CancellationToken ct = default) =>
            catalogAdminService.AddConfigAsync(request, actorUserId, ct);

        public Task<ConfigCatalogAdminResult> UpdateConfigAsync(int id, ConfigItemUpsertRequest request, int actorUserId, CancellationToken ct = default) =>
            catalogAdminService.UpdateConfigAsync(id, request, actorUserId, ct);

        public Task<ConfigCatalogAdminResult> DeleteConfigAsync(int id, CancellationToken ct = default) =>
            catalogAdminService.DeleteConfigAsync(id, ct);
    }
}
