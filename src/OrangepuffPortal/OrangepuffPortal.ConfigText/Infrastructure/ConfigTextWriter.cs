using Microsoft.Extensions.Logging;
using OrangepuffPortal.ConfigText.Contract;
using OrangepuffPortal.ConfigText.Contract.Interfaces;
using OrangepuffPortal.ConfigText.Domain.Entity;
using OrangepuffPortal.ConfigText.Domain.Repositories;

namespace OrangepuffPortal.ConfigText.Infrastructure;

/// <summary>
/// Seeds a consuming app's default text into [configtext].[ConfigTextDefinition] at that app's own startup.
/// Insert-only unless a seed entry sets <see cref="ConfigTextSeedEntry.BtReplace"/> — see the design doc (docs/config-text-design.md) for the full seeding contract.
/// </summary>
internal class ConfigTextWriter(IConfigTextRepository repository, ConfigTextCache cache, ILogger<ConfigTextWriter> logger) : IConfigTextWriter
{
    public async Task UpsertManyAsync(string cultureCode, IReadOnlyCollection<ConfigTextSeedEntry> entries, CancellationToken cancellationToken = default)
    {
        const string LogPrefix = nameof(ConfigTextWriter) + "." + nameof(UpsertManyAsync);
        logger.LogInformation("{LogPrefix}: seeding {Count} entries for culture '{Culture}'", LogPrefix, entries.Count, cultureCode);

        if (string.IsNullOrWhiteSpace(cultureCode) || cultureCode == ConfigTextDefinition.WildcardCulture)
        {
            throw new ArgumentException("CultureCode must be a real culture, not the wildcard.", nameof(cultureCode));
        }

        var utcNow = DateTime.UtcNow;
        var insertedCount = 0;
        var replacedCount = 0;

        foreach (var entry in entries)
        {
            foreach (var targetCulture in new[] { cultureCode, ConfigTextDefinition.WildcardCulture })
            {
                var existing = await repository.FindAsync(entry.SModule, entry.STextCode, targetCulture, entry.STextType, cancellationToken);

                if (existing is null)
                {
                    await repository.AddAsync(
                        new ConfigTextDefinition(entry.SModule, entry.STextCode, targetCulture, entry.STextType, entry.SText, entry.SNote, utcNow),
                        cancellationToken);
                    insertedCount++;
                }
                else if (entry.BtReplace)
                {
                    existing.Replace(entry.SText, entry.SNote, utcNow);
                    replacedCount++;
                }
            }
        }

        if (insertedCount == 0 && replacedCount == 0)
        {
            logger.LogInformation("{LogPrefix}: nothing to write for culture {Culture}, {Count} entries already up to date", LogPrefix, cultureCode, entries.Count);
            return;
        }

        await repository.SaveChangesAsync(cancellationToken);
        await cache.InvalidateAsync(cancellationToken);

        logger.LogInformation(
            "{LogPrefix}: culture {Culture} — inserted {Inserted} row(s), replaced {Replaced} row(s) out of {Count} seed entries",
            LogPrefix, cultureCode, insertedCount, replacedCount, entries.Count);
    }
}
