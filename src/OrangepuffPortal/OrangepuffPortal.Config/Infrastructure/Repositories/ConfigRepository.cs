using Microsoft.EntityFrameworkCore;
using OrangepuffPortal.Config.Domain.Entity;
using OrangepuffPortal.Config.Domain.Repositories;
using OrangepuffPortal.Shared.Paging;
using System.Linq.Expressions;

namespace OrangepuffPortal.Config.Infrastructure.Repositories;

public class ConfigRepository(ConfigDbContext db) : IConfigRepository
{
    public Task<ConfigSection?> FindSectionAsync(string textCode, CancellationToken cancellationToken = default) =>
        db.ConfigSections.FirstOrDefaultAsync(x => x.TextCode == textCode, cancellationToken);

    public Task<ConfigSection?> GetSectionByIdAsync(int id, CancellationToken cancellationToken = default) =>
        db.ConfigSections.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ConfigSection>> ListSectionsAsync(CancellationToken cancellationToken = default) =>
        await db.ConfigSections.AsNoTracking()
            .OrderBy(x => x.SortOrder ?? int.MaxValue).ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsSectionAsync(string textCode, int? excludeId, CancellationToken cancellationToken = default) =>
        db.ConfigSections.AnyAsync(x => x.TextCode == textCode && x.Id != (excludeId ?? -1), cancellationToken);

    public async Task<int?> GetMaxSectionSortOrderAsync(CancellationToken cancellationToken = default)
    {
        var orders = db.ConfigSections.Where(x => x.SortOrder != null).Select(x => x.SortOrder!.Value);
        return await orders.AnyAsync(cancellationToken) ? await orders.MaxAsync(cancellationToken) : null;
    }

    public async Task AddSectionAsync(ConfigSection section, CancellationToken cancellationToken = default) =>
        await db.ConfigSections.AddAsync(section, cancellationToken);

    public Task DeleteSectionAsync(ConfigSection section, CancellationToken cancellationToken = default)
    {
        db.ConfigSections.Remove(section);
        return Task.CompletedTask;
    }

    public Task<ConfigItem?> FindConfigByCodeAsync(string configCode, CancellationToken cancellationToken = default) =>
        db.Configs.FirstOrDefaultAsync(x => x.ConfigCode == configCode, cancellationToken);

    public Task<ConfigItem?> GetConfigByIdAsync(int id, CancellationToken cancellationToken = default) =>
        db.Configs.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<PagedResult<ConfigItem>> ListConfigsAsync(
        int? sectionId, string? configCode, string? configName, int? configType, string? sortBy, bool sortDescending, int skip, int take, CancellationToken cancellationToken = default)
    {
        var query = db.Configs.AsNoTracking().AsQueryable();

        if (sectionId.HasValue)
        {
            query = query.Where(x => x.SectionId == sectionId.Value);
        }

        if (!string.IsNullOrWhiteSpace(configCode))
        {
            query = query.Where(x => x.ConfigCode.Contains(configCode));
        }

        if (!string.IsNullOrWhiteSpace(configName))
        {
            query = query.Where(x => x.ConfigName.Contains(configName));
        }

        if (configType.HasValue)
        {
            query = query.Where(x => x.ConfigType == configType.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // "section" sorts by the parent ConfigSections.sSectionDesc via a correlated subquery rather
        // than SectionId, since that's what the admin grid actually displays in that column.
        IOrderedQueryable<ConfigItem> ordered = sortBy switch
        {
            "section" => Order(query, x => db.ConfigSections.Where(s => s.Id == x.SectionId).Select(s => s.SectionDesc).FirstOrDefault(), sortDescending),
            "configCode" => Order(query, x => x.ConfigCode, sortDescending),
            "configName" => Order(query, x => x.ConfigName, sortDescending),
            "configType" => Order(query, x => x.ConfigType, sortDescending),
            "show" => Order(query, x => x.Show, sortDescending),
            "allowUserEdit" => Order(query, x => x.AllowUserEdit, sortDescending),
            "sortOrder" => Order(query, x => x.SortOrder ?? int.MaxValue, sortDescending),
            _ => query.OrderBy(x => x.SectionId).ThenBy(x => x.SortOrder ?? int.MaxValue)
        };

        var items = await ordered
            .ThenBy(x => x.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new PagedResult<ConfigItem>(items, totalCount);
    }

    private static IOrderedQueryable<ConfigItem> Order<TKey>(IQueryable<ConfigItem> query, Expression<Func<ConfigItem, TKey>> keySelector, bool descending) =>
        descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);

    public Task<bool> ExistsConfigCodeAsync(string configCode, int? excludeId, CancellationToken cancellationToken = default) =>
        db.Configs.AnyAsync(x => x.ConfigCode == configCode && x.Id != (excludeId ?? -1), cancellationToken);

    public Task<bool> HasConfigsForSectionAsync(int sectionId, CancellationToken cancellationToken = default) =>
        db.Configs.AnyAsync(x => x.SectionId == sectionId, cancellationToken);

    public async Task<int?> GetMaxConfigSortOrderAsync(int sectionId, CancellationToken cancellationToken = default)
    {
        var orders = db.Configs.Where(x => x.SectionId == sectionId && x.SortOrder != null).Select(x => x.SortOrder!.Value);
        return await orders.AnyAsync(cancellationToken) ? await orders.MaxAsync(cancellationToken) : null;
    }

    public async Task<IReadOnlyList<ConfigItem>> ListConfigsWithDefaultAsync(CancellationToken cancellationToken = default) =>
        await db.Configs.AsNoTracking()
            .Where(x => x.DefaultStringValue != null || x.DefaultIntValue != null || x.DefaultDecimalValue != null || x.DefaultBoolValue != null)
            .ToListAsync(cancellationToken);

    public async Task AddConfigAsync(ConfigItem config, CancellationToken cancellationToken = default) =>
        await db.Configs.AddAsync(config, cancellationToken);

    public Task DeleteConfigAsync(ConfigItem config, CancellationToken cancellationToken = default)
    {
        db.Configs.Remove(config);
        return Task.CompletedTask;
    }

    public Task<ConfigUser?> FindUserValueAsync(int userId, int configId, CancellationToken cancellationToken = default) =>
        db.ConfigUsers.FirstOrDefaultAsync(x => x.UserId == userId && x.ConfigId == configId, cancellationToken);

    public async Task<IReadOnlyList<(ConfigUser Value, ConfigItem Config)>> ListUserValuesWithConfigAsync(int userId, CancellationToken cancellationToken = default) =>
        await db.ConfigUsers.AsNoTracking()
            .Where(x => x.UserId == userId)
            .Join(db.Configs, cu => cu.ConfigId, c => c.Id, (cu, c) => new ValueTuple<ConfigUser, ConfigItem>(cu, c))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<(ConfigSection Section, ConfigItem Item, ConfigUser? Value)>> ListVisibleCatalogWithUserValuesAsync(int userId, CancellationToken cancellationToken = default) =>
        await db.ConfigSections.AsNoTracking()
            .Where(section => section.Show)
            .Join(db.Configs.AsNoTracking().Where(item => item.Show), section => section.Id, item => item.SectionId, (section, item) => new { section, item })
            .GroupJoin(db.ConfigUsers.AsNoTracking().Where(value => value.UserId == userId), row => row.item.Id, value => value.ConfigId, (row, values) => new { row.section, row.item, values })
            .SelectMany(row => row.values.DefaultIfEmpty(), (row, value) => new { row.section, row.item, value })
            .OrderBy(row => row.section.SortOrder ?? int.MaxValue).ThenBy(row => row.section.Id)
            .ThenBy(row => row.item.SortOrder ?? int.MaxValue).ThenBy(row => row.item.Id)
            .Select(row => new ValueTuple<ConfigSection, ConfigItem, ConfigUser?>(row.section, row.item, row.value))
            .ToListAsync(cancellationToken);

    public async Task AddUserValueAsync(ConfigUser configUser, CancellationToken cancellationToken = default) =>
        await db.ConfigUsers.AddAsync(configUser, cancellationToken);

    public async Task AddUserValuesAsync(IReadOnlyCollection<ConfigUser> configUsers, CancellationToken cancellationToken = default) =>
        await db.ConfigUsers.AddRangeAsync(configUsers, cancellationToken);

    public async Task AddUserValueHistoryAsync(ConfigUserHistory history, CancellationToken cancellationToken = default) =>
        await db.ConfigUsersHistory.AddAsync(history, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => db.SaveChangesAsync(cancellationToken);
}
