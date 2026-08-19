using Microsoft.Extensions.Logging;
using OrangepuffPortal.ConfigText.Contract;
using OrangepuffPortal.ConfigText.Contract.Interfaces;
using OrangepuffPortal.ConfigText.Domain.Repositories;

namespace OrangepuffPortal.ConfigText.Infrastructure;

/// <summary>
/// Resolves each (module, code, type) key to a single row: the exact-culture row if one exists,
/// otherwise the "*" fallback row. Backed by <see cref="ConfigTextCache"/>.
/// </summary>
internal class ConfigTextReader(IConfigTextRepository repository, ConfigTextCache cache, ILogger<ConfigTextReader> logger) : IConfigTextReader
{
    public async Task<IReadOnlyList<ConfigTextEntryDto>> GetAllAsync(
        string cultureCode, IReadOnlyCollection<string>? modules = null, CancellationToken cancellationToken = default)
    {
        const string LogPrefix = nameof(ConfigTextReader) + "." + nameof(GetAllAsync);
        logger.LogInformation("{LogPrefix}: resolving culture '{Culture}', modules={Modules}", LogPrefix, cultureCode, modules is { Count: > 0 } ? string.Join(",", modules) : "*");

        var rows = await cache.GetOrCreateAsync(cultureCode, async () =>
        {
            var entities = await repository.GetForCultureAsync(cultureCode, cancellationToken);
            return entities
                .Select(e => new ConfigTextCacheRow(e.Module, e.TextCode, e.CultureCode, e.TextType, e.Text))
                .ToList();
        }, cancellationToken);

        // The cache always holds every module's rows for this culture (one cache entry per culture,
        // not per caller's module filter, to avoid fragmenting the cache by arbitrary module-set
        // combinations) — filtering to the caller's requested modules happens here, in-memory, after
        // the cache hit.
        var filtered = modules is { Count: > 0 }
            ? rows.Where(x => modules.Contains(x.Module))
            : rows;

        var result = filtered
            .GroupBy(x => (x.Module, x.TextCode, x.TextType))
            .Select(g => g.OrderBy(x => x.CultureCode == cultureCode ? 0 : 1).First())
            .Select(x => new ConfigTextEntryDto(x.Module, x.TextCode, x.TextType, x.Text))
            .ToList();

        logger.LogInformation("{LogPrefix}: returned {Count} entries for culture '{Culture}'", LogPrefix, result.Count, cultureCode);
        return result;
    }
}
