using OrangepuffPortal.ConfigData.Contract;
using OrangepuffPortal.ConfigData.Contract.Interfaces;
using OrangepuffPortal.ConfigData.Domain.Entity;
using OrangepuffPortal.ConfigData.Domain.Repositories;

namespace OrangepuffPortal.ConfigData.Infrastructure;

/// <summary>
/// Read-only <see cref="IConfigDataService"/> that serves values from <see cref="ConfigDataCache"/>,
/// falling back to the DB on a cache miss and repopulating the cache for subsequent reads.
/// </summary>
internal sealed class ConfigDataService(ConfigDataCache cache, IConfigDataRepository repository) : IConfigDataService
{
    public async Task<string?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        var all = await GetCachedAsync(cancellationToken);
        return all.FirstOrDefault(r => string.Equals(r.Key, key, StringComparison.OrdinalIgnoreCase))?.Value;
    }

    public async Task<IReadOnlyList<ConfigDataDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var all = await GetCachedAsync(cancellationToken);
        return all.Select(r => new ConfigDataDto(r.Id, r.Key, r.Value, r.AllowEditByScreen, r.Description)).ToList();
    }

    private async Task<IReadOnlyList<ConfigDataCacheRow>> GetCachedAsync(CancellationToken cancellationToken)
    {
        var cached = await cache.GetAsync(cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var entities = await repository.GetAllAsync(cancellationToken);
        var rows = entities.Select(ToRow).ToList();
        await cache.SetAllAsync(rows, cancellationToken);
        return rows;
    }

    private static ConfigDataCacheRow ToRow(ConfigDataEntry e) =>
        new(e.Id, e.Key, e.Value, e.AllowEditByScreen, e.Description);
}
