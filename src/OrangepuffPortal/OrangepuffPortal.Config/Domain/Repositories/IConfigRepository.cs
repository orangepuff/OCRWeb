using OrangepuffPortal.Config.Domain.Entity;
using OrangepuffPortal.Shared.Paging;

namespace OrangepuffPortal.Config.Domain.Repositories;

public interface IConfigRepository
{
    // Catalog — sections
    Task<ConfigSection?> FindSectionAsync(string textCode, CancellationToken cancellationToken = default);

    Task<ConfigSection?> GetSectionByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Unpaged — the catalog's sections are expected to stay a small, hand-curated list.</summary>
    Task<IReadOnlyList<ConfigSection>> ListSectionsAsync(CancellationToken cancellationToken = default);

    Task<bool> ExistsSectionAsync(string textCode, int? excludeId, CancellationToken cancellationToken = default);

    /// <summary>Highest SortOrder set across every section (null-valued rows ignored), or null if none is set.</summary>
    Task<int?> GetMaxSectionSortOrderAsync(CancellationToken cancellationToken = default);

    Task AddSectionAsync(ConfigSection section, CancellationToken cancellationToken = default);

    Task DeleteSectionAsync(ConfigSection section, CancellationToken cancellationToken = default);

    // Catalog — configs
    Task<ConfigItem?> FindConfigByCodeAsync(string configCode, CancellationToken cancellationToken = default);

    Task<ConfigItem?> GetConfigByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// <paramref name="sortBy"/> is one of: "section", "configCode", "configName", "configType",
    /// "show", "allowUserEdit", "sortOrder" — anything else (including null) falls back to the
    /// default ordering (section, then sort order, then id).
    /// </summary>
    Task<PagedResult<ConfigItem>> ListConfigsAsync(
        int? sectionId, string? configCode, string? configName, int? configType, string? sortBy, bool sortDescending, int skip, int take, CancellationToken cancellationToken = default);

    Task<bool> ExistsConfigCodeAsync(string configCode, int? excludeId, CancellationToken cancellationToken = default);

    Task<bool> HasConfigsForSectionAsync(int sectionId, CancellationToken cancellationToken = default);

    /// <summary>Highest SortOrder set among a section's configs (null-valued rows ignored), or null if none is set.</summary>
    Task<int?> GetMaxConfigSortOrderAsync(int sectionId, CancellationToken cancellationToken = default);

    /// <summary>Every config that has at least one default value field set — used to apply defaults onto a newly-created user.</summary>
    Task<IReadOnlyList<ConfigItem>> ListConfigsWithDefaultAsync(CancellationToken cancellationToken = default);

    Task AddConfigAsync(ConfigItem config, CancellationToken cancellationToken = default);

    Task DeleteConfigAsync(ConfigItem config, CancellationToken cancellationToken = default);

    // Values
    Task<ConfigUser?> FindUserValueAsync(int userId, int configId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<(ConfigUser Value, ConfigItem Config)>> ListUserValuesWithConfigAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>Every visible section/config, left-joined to <paramref name="userId"/>'s value if any, ordered by SortOrder (nulls last) then Id at both levels.</summary>
    Task<IReadOnlyList<(ConfigSection Section, ConfigItem Item, ConfigUser? Value)>> ListVisibleCatalogWithUserValuesAsync(int userId, CancellationToken cancellationToken = default);

    Task AddUserValueAsync(ConfigUser configUser, CancellationToken cancellationToken = default);

    /// <summary>Bulk insert — used to backfill a brand-new config's default onto every existing user in one go.</summary>
    Task AddUserValuesAsync(IReadOnlyCollection<ConfigUser> configUsers, CancellationToken cancellationToken = default);

    Task AddUserValueHistoryAsync(ConfigUserHistory history, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
