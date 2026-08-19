using Microsoft.Extensions.Logging;
using OrangepuffPortal.ConfigData.Contract;
using OrangepuffPortal.ConfigData.Contract.Interfaces;
using OrangepuffPortal.ConfigData.Domain.Entity;
using OrangepuffPortal.ConfigData.Domain.Repositories;
using OrangepuffPortal.Shared.Translation;

namespace OrangepuffPortal.ConfigData.Infrastructure;

/// <summary>
/// Admin CRUD over [configdata].[ConfigData], called from the Bff admin group.
/// Every mutation invalidates the Redis cache so <see cref="ConfigDataService"/> picks up
/// the change on the next read.
/// </summary>
internal class ConfigDataAdminService(
    IConfigDataRepository repository,
    ConfigDataCache cache,
    ITranslation translation,
    ILogger<ConfigDataAdminService> logger) : IConfigDataAdminService
{
    private const string ModuleName = "OrangepuffPortal.ConfigData";

    private Task<string> TranslateAsync(string reasonCode, CancellationToken ct) =>
        translation.TranslateAsync(reasonCode, ModuleName, ct);

    public async Task<IReadOnlyList<ConfigDataDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var all = await repository.GetAllAsync(cancellationToken);
        return all.Select(ToDto).ToList();
    }

    public async Task<ConfigDataAdminResult> CreateAsync(ConfigDataUpsertRequest request, int actorUserId, CancellationToken cancellationToken = default)
    {
        const string LogPrefix = nameof(ConfigDataAdminService) + "." + nameof(CreateAsync);

        if (await repository.ExistsAsync(request.SKey, null, cancellationToken))
        {
            logger.LogWarning("{LogPrefix}: rejected, duplicate key {Key}", LogPrefix, request.SKey);
            return ConfigDataAdminResult.Rejected(await TranslateAsync("duplicate_key", cancellationToken));
        }

        var entry = new ConfigDataEntry(request.SKey, request.SValue, request.BAllowEditByScreen, request.SDescription, DateTime.UtcNow, actorUserId);
        await repository.AddAsync(entry, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        await cache.InvalidateAsync(cancellationToken);

        logger.LogInformation("{LogPrefix}: created row {Id}", LogPrefix, entry.Id);
        return ConfigDataAdminResult.Created(entry.Id, await TranslateAsync("configdata_created", cancellationToken));
    }

    public async Task<ConfigDataAdminResult> UpdateAsync(int id, ConfigDataUpsertRequest request, int actorUserId, CancellationToken cancellationToken = default)
    {
        const string LogPrefix = nameof(ConfigDataAdminService) + "." + nameof(UpdateAsync);

        var entry = await repository.GetByIdAsync(id, cancellationToken);
        if (entry is null)
        {
            logger.LogWarning("{LogPrefix}: row {Id} not found", LogPrefix, id);
            return ConfigDataAdminResult.Rejected(await TranslateAsync("not_found", cancellationToken));
        }

        if (await repository.ExistsAsync(request.SKey, id, cancellationToken))
        {
            logger.LogWarning("{LogPrefix}: rejected, duplicate key {Key}", LogPrefix, request.SKey);
            return ConfigDataAdminResult.Rejected(await TranslateAsync("duplicate_key", cancellationToken));
        }

        entry.AdminUpdate(request.SKey, request.SValue, request.BAllowEditByScreen, request.SDescription, actorUserId, DateTime.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
        await cache.InvalidateAsync(cancellationToken);

        logger.LogInformation("{LogPrefix}: updated row {Id}", LogPrefix, id);
        return ConfigDataAdminResult.Updated(id, await TranslateAsync("configdata_updated", cancellationToken));
    }

    public async Task<ConfigDataAdminResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        const string LogPrefix = nameof(ConfigDataAdminService) + "." + nameof(DeleteAsync);

        var entry = await repository.GetByIdAsync(id, cancellationToken);
        if (entry is null)
        {
            logger.LogWarning("{LogPrefix}: row {Id} not found", LogPrefix, id);
            return ConfigDataAdminResult.Rejected(await TranslateAsync("not_found", cancellationToken));
        }

        await repository.DeleteAsync(entry, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        await cache.InvalidateAsync(cancellationToken);

        logger.LogInformation("{LogPrefix}: deleted row {Id}", LogPrefix, id);
        return ConfigDataAdminResult.Deleted(await TranslateAsync("configdata_deleted", cancellationToken));
    }

    private static ConfigDataDto ToDto(ConfigDataEntry e) =>
        new(e.Id, e.Key, e.Value, e.AllowEditByScreen, e.Description);
}
