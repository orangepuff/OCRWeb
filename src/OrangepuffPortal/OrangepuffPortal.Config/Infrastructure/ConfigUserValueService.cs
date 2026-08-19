using Microsoft.Extensions.Logging;
using OrangepuffPortal.Config.Contract;
using OrangepuffPortal.Config.Contract.Interfaces;
using OrangepuffPortal.Config.Domain.Entity;
using OrangepuffPortal.Config.Domain.Repositories;
using OrangepuffPortal.Shared.Auditing;

namespace OrangepuffPortal.Config.Infrastructure;

/// <summary>
/// Read/write path for a user's own config values — live application/user data, never seeded.
/// <see cref="SetValueAsync"/> snapshots the current value into ConfigUsersHistory before overwriting it.
/// </summary>
internal class ConfigUserValueService(
    IConfigRepository repository,
    ICurrentUser currentUser,
    IUserDirectory userDirectory,
    UserConfigCache userConfigCache,
    ILogger<ConfigUserValueService> logger) : IConfigUserValueService
{
    public async Task<ConfigUserValueDto?> GetValueAsync(int userId, string configCode, CancellationToken cancellationToken = default)
    {
        var config = await GetConfigOrThrowAsync(configCode, cancellationToken);
        var value = await repository.FindUserValueAsync(userId, config.Id, cancellationToken);
        return value is null ? null : ToDto(config, value);
    }

    public async Task<IReadOnlyList<ConfigUserValueDto>> GetValuesAsync(int userId, CancellationToken cancellationToken = default)
    {
        var rows = await repository.ListUserValuesWithConfigAsync(userId, cancellationToken);
        return rows.Select(row => ToDto(row.Config, row.Value)).ToList();
    }

    public async Task<IReadOnlyList<UserConfigSectionDto>> GetSectionsForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var rows = await repository.ListVisibleCatalogWithUserValuesAsync(userId, cancellationToken);

        return rows
            .GroupBy(row => row.Section.Id)
            .Select(group =>
            {
                var section = group.First().Section;
                var items = group.Select(row => new UserConfigItemDto(
                    row.Item.ConfigCode,
                    row.Item.ConfigName,
                    row.Item.TextCode,
                    (ConfigValueType)row.Item.ConfigType,
                    row.Item.AllowUserEdit,
                    row.Item.SortOrder,
                    row.Value?.StringValue,
                    row.Value?.IntValue,
                    row.Value?.DecimalValue,
                    row.Value?.BoolValue)).ToList();

                return new UserConfigSectionDto(section.SectionDesc, section.TextCode, section.SortOrder, items);
            })
            .ToList();
    }

    public async Task SetValueAsync(int userId, string configCode, ConfigValueInput value, CancellationToken cancellationToken = default)
    {
        const string LogPrefix = nameof(ConfigUserValueService) + "." + nameof(SetValueAsync);

        var config = await GetConfigOrThrowAsync(configCode, cancellationToken);
        var utcNow = DateTime.UtcNow;
        var actorUserId = currentUser.UserId;

        var existing = await repository.FindUserValueAsync(userId, config.Id, cancellationToken);

        if (existing is null)
        {
            var created = new ConfigUser(userId, config.Id, value.SConfigValue, value.IConfigValue, value.NConfigValue, value.BtConfigValue, actorUserId, utcNow);
            await repository.AddUserValueAsync(created, cancellationToken);
            logger.LogInformation("{LogPrefix}: created value for user {UserId}, config {ConfigCode}", LogPrefix, userId, configCode);
        }
        else
        {
            var snapshot = new ConfigUserHistory(
                existing.Id, existing.UserId, existing.ConfigId,
                existing.StringValue, existing.IntValue, existing.DecimalValue, existing.BoolValue, existing.Active,
                actorUserId, utcNow);
            await repository.AddUserValueHistoryAsync(snapshot, cancellationToken);

            existing.SetValue(value.SConfigValue, value.IConfigValue, value.NConfigValue, value.BtConfigValue, actorUserId, utcNow);
            logger.LogInformation("{LogPrefix}: updated value for user {UserId}, config {ConfigCode}", LogPrefix, userId, configCode);
        }

        await repository.SaveChangesAsync(cancellationToken);

        // Otherwise ICurrentUserConfig would keep serving the pre-update value out of the cache warmed
        // at this user's login until the cache entry's sliding expiration lapses.
        await userConfigCache.InvalidateAsync(userId, cancellationToken);
    }

    public async Task ApplyDefaultsForNewUserAsync(int userId, int actorUserId, CancellationToken cancellationToken = default)
    {
        const string LogPrefix = nameof(ConfigUserValueService) + "." + nameof(ApplyDefaultsForNewUserAsync);

        var utcNow = DateTime.UtcNow;
        var configsWithDefault = await repository.ListConfigsWithDefaultAsync(cancellationToken);

        var applied = 0;
        foreach (var config in configsWithDefault)
        {
            if (await repository.FindUserValueAsync(userId, config.Id, cancellationToken) is not null)
            {
                continue;
            }

            var created = new ConfigUser(userId, config.Id, config.DefaultStringValue, config.DefaultIntValue, config.DefaultDecimalValue, config.DefaultBoolValue, actorUserId, utcNow);
            await repository.AddUserValueAsync(created, cancellationToken);
            applied++;
        }

        if (applied > 0)
        {
            await repository.SaveChangesAsync(cancellationToken);
            logger.LogInformation("{LogPrefix}: applied {Count} config default(s) to new user {UserId}", LogPrefix, applied, userId);
        }
        else
        {
            logger.LogDebug("{LogPrefix}: no config defaults to apply to user {UserId}", LogPrefix, userId);
        }
    }

    public async Task ApplyDefaultForNewConfigAsync(int configId, int actorUserId, CancellationToken cancellationToken = default)
    {
        const string LogPrefix = nameof(ConfigUserValueService) + "." + nameof(ApplyDefaultForNewConfigAsync);

        var config = await repository.GetConfigByIdAsync(configId, cancellationToken);
        if (config is null)
        {
            logger.LogWarning("{LogPrefix}: config {ConfigId} not found, nothing to backfill", LogPrefix, configId);
            return;
        }

        if (config.DefaultStringValue is null && config.DefaultIntValue is null && config.DefaultDecimalValue is null && config.DefaultBoolValue is null)
        {
            logger.LogDebug("{LogPrefix}: config {ConfigId} has no default value, nothing to backfill", LogPrefix, configId);
            return;
        }

        var utcNow = DateTime.UtcNow;
        var userIds = await userDirectory.GetAllUserIdsAsync(cancellationToken);

        var toCreate = new List<ConfigUser>();
        foreach (var userId in userIds)
        {
            if (await repository.FindUserValueAsync(userId, config.Id, cancellationToken) is not null)
            {
                continue;
            }

            toCreate.Add(new ConfigUser(userId, config.Id, config.DefaultStringValue, config.DefaultIntValue, config.DefaultDecimalValue, config.DefaultBoolValue, actorUserId, utcNow));
        }

        if (toCreate.Count > 0)
        {
            await repository.AddUserValuesAsync(toCreate, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);
            logger.LogInformation("{LogPrefix}: backfilled config {ConfigId}'s default onto {Count} existing user(s)", LogPrefix, config.Id, toCreate.Count);
        }
        else
        {
            logger.LogDebug("{LogPrefix}: config {ConfigId} had no existing users left to backfill", LogPrefix, config.Id);
        }
    }

    private async Task<ConfigItem> GetConfigOrThrowAsync(string configCode, CancellationToken cancellationToken)
    {
        const string LogPrefix = nameof(ConfigUserValueService) + "." + nameof(GetConfigOrThrowAsync);

        var config = await repository.FindConfigByCodeAsync(configCode, cancellationToken);
        if (config is null)
        {
            logger.LogError("{LogPrefix}: config '{ConfigCode}' does not exist", LogPrefix, configCode);
            throw new InvalidOperationException($"Config '{configCode}' does not exist.");
        }

        return config;
    }

    private static ConfigUserValueDto ToDto(ConfigItem config, ConfigUser value) =>
        new(config.ConfigCode, (ConfigValueType)config.ConfigType, value.StringValue, value.IntValue, value.DecimalValue, value.BoolValue);
}
