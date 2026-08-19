using Microsoft.Extensions.Logging;
using OrangepuffPortal.Config.Contract;
using OrangepuffPortal.Config.Contract.Interfaces;
using OrangepuffPortal.Config.Domain;
using OrangepuffPortal.Config.Domain.Entity;
using OrangepuffPortal.Config.Domain.Repositories;

namespace OrangepuffPortal.Config.Infrastructure;

/// <summary>
/// Seeds a consuming app's default sections/configs into [config].[ConfigSections]/[Configs] at that
/// app's own startup. Insert-only unless a seed entry sets BtReplace — see the design doc
/// (docs/config-settings-design.md) for the full seeding contract.
/// </summary>
internal class ConfigCatalogWriter(IConfigRepository repository, ILogger<ConfigCatalogWriter> logger) : IConfigCatalogWriter
{
    public async Task UpsertAsync(string module, IReadOnlyCollection<ConfigSectionSeedEntry> sections, IReadOnlyList<int>? existingUserIds = null, CancellationToken cancellationToken = default)
    {
        const string LogPrefix = nameof(ConfigCatalogWriter) + "." + nameof(UpsertAsync);

        var utcNow = DateTime.UtcNow;
        var sectionsWritten = 0;
        var configsWritten = 0;
        var newConfigsWithDefaults = new List<(ConfigItem Config, ConfigValueInput DefaultValue)>();

        foreach (var sectionEntry in sections)
        {
            var section = await repository.FindSectionAsync(sectionEntry.STextCode, cancellationToken);

            if (section is null)
            {
                section = new ConfigSection(sectionEntry.SSectionDesc, sectionEntry.STextCode, sectionEntry.BtShow, utcNow, sectionEntry.ISortOrder);
                await repository.AddSectionAsync(section, cancellationToken);
                // Configs below need section.Id for their FK, so this one flushes early.
                await repository.SaveChangesAsync(cancellationToken);
                sectionsWritten++;
            }
            else if (sectionEntry.BtReplace)
            {
                section.Replace(sectionEntry.SSectionDesc, sectionEntry.BtShow, utcNow, sectionEntry.ISortOrder);
                sectionsWritten++;
            }

            foreach (var configEntry in sectionEntry.Configs)
            {
                var config = await repository.FindConfigByCodeAsync(configEntry.SConfigCode, cancellationToken);

                if (config is null)
                {
                    config = new ConfigItem(
                        section.Id, configEntry.SConfigCode, configEntry.SConfigName, configEntry.STextCode, configEntry.IConfigType, configEntry.BtShow, configEntry.BtAllowUserEdit, utcNow, configEntry.ISortOrder,
                        configEntry.DefaultValue?.SConfigValue, configEntry.DefaultValue?.IConfigValue, configEntry.DefaultValue?.NConfigValue, configEntry.DefaultValue?.BtConfigValue);
                    await repository.AddConfigAsync(config, cancellationToken);
                    configsWritten++;

                    if (configEntry.DefaultValue is not null)
                    {
                        newConfigsWithDefaults.Add((config, configEntry.DefaultValue));
                    }
                }
                else if (configEntry.BtReplace)
                {
                    // Updates the stored default for future new users, but deliberately does not
                    // retroactively touch any existing user's ConfigUsers value — see ConfigSeedEntry.DefaultValue.
                    config.Replace(
                        section.Id, configEntry.SConfigName, configEntry.STextCode, configEntry.IConfigType, configEntry.BtShow, configEntry.BtAllowUserEdit, utcNow, configEntry.ISortOrder,
                        configEntry.DefaultValue?.SConfigValue, configEntry.DefaultValue?.IConfigValue, configEntry.DefaultValue?.NConfigValue, configEntry.DefaultValue?.BtConfigValue);
                    configsWritten++;
                }
            }
        }

        if (sectionsWritten == 0 && configsWritten == 0)
        {
            logger.LogInformation("{LogPrefix}: nothing new to write for module {Module}", LogPrefix, module);
            return;
        }

        await repository.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "{LogPrefix}: module {Module} — wrote {Sections} section(s), {Configs} config(s)",
            LogPrefix, module, sectionsWritten, configsWritten);

        if (newConfigsWithDefaults.Count == 0 || existingUserIds is not { Count: > 0 })
        {
            return;
        }

        // Config.Id is only populated now, after the SaveChangesAsync above — this pass has to come after it.
        var backfilled = new List<ConfigUser>();
        foreach (var (newConfig, defaultValue) in newConfigsWithDefaults)
        {
            foreach (var userId in existingUserIds)
            {
                backfilled.Add(new ConfigUser(
                    userId, newConfig.Id, defaultValue.SConfigValue, defaultValue.IConfigValue, defaultValue.NConfigValue, defaultValue.BtConfigValue,
                    ConfigConstants.SystemActorUserId, utcNow));
            }
        }

        await repository.AddUserValuesAsync(backfilled, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "{LogPrefix}: module {Module} — backfilled {Count} default value(s) across {ConfigCount} new config(s) onto {UserCount} existing user(s)",
            LogPrefix, module, backfilled.Count, newConfigsWithDefaults.Count, existingUserIds.Count);
    }
}
