using Microsoft.Extensions.Logging;
using OrangepuffPortal.ConfigText.Contract;
using OrangepuffPortal.ConfigText.Contract.Interfaces;
using OrangepuffPortal.ConfigText.Domain.Entity;
using OrangepuffPortal.ConfigText.Domain.Repositories;
using OrangepuffPortal.Shared.Paging;
using OrangepuffPortal.Shared.Translation;

namespace OrangepuffPortal.ConfigText.Infrastructure;

/// <summary>
/// Admin CRUD over [configtext].[ConfigTextDefinition], driven by a signed-in admin from the Bff —
/// unlike <see cref="ConfigTextWriter"/>, which is only ever run by unattended startup seeding.
/// </summary>
internal class ConfigTextAdminService(IConfigTextRepository repository, ConfigTextCache cache, ITranslation translation, ILogger<ConfigTextAdminService> logger) : IConfigTextAdminService
{
    private const string ModuleName = "OrangepuffPortal.ConfigText";

    private Task<string> TranslateAsync(string reasonCode, CancellationToken cancellationToken) => translation.TranslateAsync(reasonCode, ModuleName, cancellationToken);

    public async Task<PagedResult<ConfigTextAdminDto>> ListAsync(
        string? module, string? textCode, string? cultureCode, string? textType, string? text, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        const string LogPrefix = nameof(ConfigTextAdminService) + "." + nameof(ListAsync);
        logger.LogInformation("{LogPrefix}: page {Page}, pageSize {PageSize}", LogPrefix, page, pageSize);

        var skip = Math.Max(0, page - 1) * pageSize;
        var result = await repository.ListAsync(module, textCode, cultureCode, textType, text, skip, pageSize, cancellationToken);

        logger.LogInformation("{LogPrefix}: returned {Count} of {Total} rows", LogPrefix, result.Items.Count, result.TotalCount);
        return new PagedResult<ConfigTextAdminDto>(result.Items.Select(ToDto).ToList(), result.TotalCount);
    }

    public async Task<ConfigTextAdminResult> AddAsync(ConfigTextAdminUpsertRequest request, int actorUserId, CancellationToken cancellationToken = default)
    {
        const string LogPrefix = nameof(ConfigTextAdminService) + "." + nameof(AddAsync);
        logger.LogInformation("{LogPrefix}: adding {Module}/{TextCode}/{CultureCode}/{TextType}", LogPrefix, request.SModule, request.STextCode, request.SCultureCode, request.STextType);

        if (await repository.ExistsAsync(request.SModule, request.STextCode, request.SCultureCode, request.STextType, null, cancellationToken))
        {
            logger.LogWarning("{LogPrefix}: rejected, duplicate key {Module}/{TextCode}/{CultureCode}/{TextType}", LogPrefix, request.SModule, request.STextCode, request.SCultureCode, request.STextType);
            return ConfigTextAdminResult.Rejected(await TranslateAsync("duplicate_key", cancellationToken));
        }

        var utcNow = DateTime.UtcNow;
        var entry = new ConfigTextDefinition(request.SModule, request.STextCode, request.SCultureCode, request.STextType, request.SText, request.SNote, utcNow, actorUserId);
        await repository.AddAsync(entry, cancellationToken);

        // Mirrors ConfigTextWriter's seeding contract: every real culture is expected to have a "*"
        // fallback row alongside it. An admin adding e.g. "en-US" straight from the grid (rather than
        // through a module's seed file) would otherwise silently skip that fallback, so backfill it
        // here — but only if it's not already there, and never touch it if it is.
        if (request.SCultureCode != ConfigTextDefinition.WildcardCulture &&
            !await repository.ExistsAsync(request.SModule, request.STextCode, ConfigTextDefinition.WildcardCulture, request.STextType, null, cancellationToken))
        {
            var wildcardEntry = new ConfigTextDefinition(
                request.SModule, request.STextCode, ConfigTextDefinition.WildcardCulture, request.STextType, request.SText, request.SNote, utcNow, actorUserId);
            await repository.AddAsync(wildcardEntry, cancellationToken);
            logger.LogInformation("{LogPrefix}: also created missing fallback row for {Module}/{TextCode}/{TextType}", LogPrefix, request.SModule, request.STextCode, request.STextType);
        }

        await repository.SaveChangesAsync(cancellationToken);
        await cache.InvalidateAsync(cancellationToken);

        logger.LogInformation("{LogPrefix}: created row {Id}", LogPrefix, entry.Id);
        return ConfigTextAdminResult.Created(entry.Id, await TranslateAsync("configtext_created", cancellationToken));
    }

    public async Task<ConfigTextAdminResult> UpdateAsync(int id, ConfigTextAdminUpsertRequest request, int actorUserId, CancellationToken cancellationToken = default)
    {
        const string LogPrefix = nameof(ConfigTextAdminService) + "." + nameof(UpdateAsync);
        logger.LogInformation("{LogPrefix}: updating row {Id}", LogPrefix, id);

        var entry = await repository.GetByIdAsync(id, cancellationToken);
        if (entry is null)
        {
            logger.LogWarning("{LogPrefix}: row {Id} not found", LogPrefix, id);
            return ConfigTextAdminResult.Rejected(await TranslateAsync("not_found", cancellationToken));
        }

        if (await repository.ExistsAsync(request.SModule, request.STextCode, request.SCultureCode, request.STextType, id, cancellationToken))
        {
            logger.LogWarning("{LogPrefix}: rejected, duplicate key {Module}/{TextCode}/{CultureCode}/{TextType}", LogPrefix, request.SModule, request.STextCode, request.SCultureCode, request.STextType);
            return ConfigTextAdminResult.Rejected(await TranslateAsync("duplicate_key", cancellationToken));
        }

        entry.AdminUpdate(request.SModule, request.STextCode, request.SCultureCode, request.STextType, request.SText, request.SNote, actorUserId, DateTime.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
        await cache.InvalidateAsync(cancellationToken);

        logger.LogInformation("{LogPrefix}: updated row {Id}", LogPrefix, id);
        return ConfigTextAdminResult.Updated(id, await TranslateAsync("configtext_updated", cancellationToken));
    }

    public async Task<ConfigTextAdminResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        const string LogPrefix = nameof(ConfigTextAdminService) + "." + nameof(DeleteAsync);
        logger.LogInformation("{LogPrefix}: deleting row {Id}", LogPrefix, id);

        var entry = await repository.GetByIdAsync(id, cancellationToken);
        if (entry is null)
        {
            logger.LogWarning("{LogPrefix}: row {Id} not found", LogPrefix, id);
            return ConfigTextAdminResult.Rejected(await TranslateAsync("not_found", cancellationToken));
        }

        await repository.DeleteAsync(entry, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        await cache.InvalidateAsync(cancellationToken);

        logger.LogInformation("{LogPrefix}: deleted row {Id}", LogPrefix, id);
        return ConfigTextAdminResult.Deleted(await TranslateAsync("configtext_deleted", cancellationToken));
    }

    private static ConfigTextAdminDto ToDto(ConfigTextDefinition entry) => new(entry.Id, entry.Module, entry.TextCode, entry.CultureCode, entry.TextType, entry.Text, entry.Note, entry.InsertedTime, entry.UpdatedTime);
}
