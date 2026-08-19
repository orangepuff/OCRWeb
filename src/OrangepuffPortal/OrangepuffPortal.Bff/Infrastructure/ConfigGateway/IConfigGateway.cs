using OrangepuffPortal.Config.Contract;
using OrangepuffPortal.Shared.Paging;

namespace OrangepuffPortal.Bff.Infrastructure.ConfigGateway
{
    /// <summary>
    /// Facade the Bff uses to reach the Config module, in-process.
    /// </summary>
    public interface IConfigGateway
    {
        Task<IReadOnlyList<UserConfigSectionDto>> GetSectionsForUserAsync(int userId, CancellationToken ct = default);
        Task SetConfigValueAsync(int userId, string configCode, ConfigValueInput value, CancellationToken ct = default);

        Task<IReadOnlyList<ConfigSectionAdminDto>> ListSectionsAsync(CancellationToken ct = default);
        Task<ConfigCatalogAdminResult> AddSectionAsync(ConfigSectionUpsertRequest request, int actorUserId, CancellationToken ct = default);
        Task<ConfigCatalogAdminResult> UpdateSectionAsync(int id, ConfigSectionUpsertRequest request, int actorUserId, CancellationToken ct = default);
        Task<ConfigCatalogAdminResult> DeleteSectionAsync(int id, CancellationToken ct = default);

        Task<PagedResult<ConfigItemAdminDto>> ListConfigsAsync(
            int? sectionId, string? configCode, string? configName, int? configType, string? sortBy, bool sortDescending, int page, int pageSize, CancellationToken ct = default);
        Task<ConfigCatalogAdminResult> AddConfigAsync(ConfigItemUpsertRequest request, int actorUserId, CancellationToken ct = default);
        Task<ConfigCatalogAdminResult> UpdateConfigAsync(int id, ConfigItemUpsertRequest request, int actorUserId, CancellationToken ct = default);
        Task<ConfigCatalogAdminResult> DeleteConfigAsync(int id, CancellationToken ct = default);
    }
}
