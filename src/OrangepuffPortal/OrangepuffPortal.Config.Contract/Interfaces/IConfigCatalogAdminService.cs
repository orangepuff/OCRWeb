using OrangepuffPortal.Shared.Paging;

namespace OrangepuffPortal.Config.Contract.Interfaces;

/// <summary>
/// Admin CRUD over the [config].[ConfigSections]/[Configs] catalog — distinct from the seed-only
/// <see cref="IConfigCatalogWriter"/>.
/// </summary>
public interface IConfigCatalogAdminService
{
    Task<IReadOnlyList<ConfigSectionAdminDto>> ListSectionsAsync(CancellationToken cancellationToken = default);

    Task<ConfigCatalogAdminResult> AddSectionAsync(ConfigSectionUpsertRequest request, int actorUserId, CancellationToken cancellationToken = default);

    Task<ConfigCatalogAdminResult> UpdateSectionAsync(int id, ConfigSectionUpsertRequest request, int actorUserId, CancellationToken cancellationToken = default);

    Task<ConfigCatalogAdminResult> DeleteSectionAsync(int id, CancellationToken cancellationToken = default);

    Task<PagedResult<ConfigItemAdminDto>> ListConfigsAsync(
        int? sectionId, string? configCode, string? configName, int? configType, string? sortBy, bool sortDescending, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<ConfigCatalogAdminResult> AddConfigAsync(ConfigItemUpsertRequest request, int actorUserId, CancellationToken cancellationToken = default);

    Task<ConfigCatalogAdminResult> UpdateConfigAsync(int id, ConfigItemUpsertRequest request, int actorUserId, CancellationToken cancellationToken = default);

    Task<ConfigCatalogAdminResult> DeleteConfigAsync(int id, CancellationToken cancellationToken = default);
}
