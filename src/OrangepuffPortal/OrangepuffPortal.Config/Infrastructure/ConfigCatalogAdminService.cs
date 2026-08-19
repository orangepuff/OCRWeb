using Microsoft.Extensions.Logging;
using OrangepuffPortal.Config.Contract;
using OrangepuffPortal.Config.Contract.Interfaces;
using OrangepuffPortal.Config.Domain.Entity;
using OrangepuffPortal.Config.Domain.Repositories;
using OrangepuffPortal.Shared.Paging;
using OrangepuffPortal.Shared.Translation;

namespace OrangepuffPortal.Config.Infrastructure;

/// <summary>
/// Admin CRUD over the [config].[ConfigSections]/[Configs] catalog, driven by a signed-in admin from
/// the Bff — unlike <see cref="ConfigCatalogWriter"/>, which is only ever run by unattended startup seeding.
/// </summary>
internal class ConfigCatalogAdminService(
    IConfigRepository repository,
    IConfigUserValueService configUserValueService,
    ITranslation translation,
    ILogger<ConfigCatalogAdminService> logger) : IConfigCatalogAdminService
{
    private const string ModuleName = "OrangepuffPortal.Config";

    private Task<string> TranslateAsync(string reasonCode, CancellationToken cancellationToken) =>
        translation.TranslateAsync(reasonCode, ModuleName, cancellationToken);

    public async Task<IReadOnlyList<ConfigSectionAdminDto>> ListSectionsAsync(CancellationToken cancellationToken = default) =>
        (await repository.ListSectionsAsync(cancellationToken)).Select(ToSectionDto).ToList();

    public async Task<ConfigCatalogAdminResult> AddSectionAsync(ConfigSectionUpsertRequest request, int actorUserId, CancellationToken cancellationToken = default)
    {
        const string LogPrefix = nameof(ConfigCatalogAdminService) + "." + nameof(AddSectionAsync);

        if (await repository.ExistsSectionAsync(request.STextCode, null, cancellationToken))
        {
            logger.LogWarning("{LogPrefix}: rejected, duplicate key {TextCode}", LogPrefix, request.STextCode);
            return ConfigCatalogAdminResult.Rejected(await TranslateAsync("duplicate_key", cancellationToken));
        }

        // Auto-assigns the next sort order when the admin leaves it blank, instead of leaving the row
        // unordered (nulls sort last, indistinguishable from "never given a position") - same reasoning
        // applies to AddConfigAsync below, one section-scoped.
        var sortOrder = request.ISortOrder ?? (await repository.GetMaxSectionSortOrderAsync(cancellationToken) ?? 0) + 1;

        var section = new ConfigSection(request.SSectionDesc, request.STextCode, request.BtShow, DateTime.UtcNow, sortOrder, actorUserId);
        await repository.AddSectionAsync(section, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("{LogPrefix}: created section {Id}", LogPrefix, section.Id);
        return ConfigCatalogAdminResult.Created(section.Id, await TranslateAsync("section_created", cancellationToken));
    }

    public async Task<ConfigCatalogAdminResult> UpdateSectionAsync(int id, ConfigSectionUpsertRequest request, int actorUserId, CancellationToken cancellationToken = default)
    {
        const string LogPrefix = nameof(ConfigCatalogAdminService) + "." + nameof(UpdateSectionAsync);

        var section = await repository.GetSectionByIdAsync(id, cancellationToken);
        if (section is null)
        {
            logger.LogWarning("{LogPrefix}: section {Id} not found", LogPrefix, id);
            return ConfigCatalogAdminResult.Rejected(await TranslateAsync("not_found", cancellationToken));
        }

        if (await repository.ExistsSectionAsync(request.STextCode, id, cancellationToken))
        {
            logger.LogWarning("{LogPrefix}: rejected, duplicate key {TextCode}", LogPrefix, request.STextCode);
            return ConfigCatalogAdminResult.Rejected(await TranslateAsync("duplicate_key", cancellationToken));
        }

        section.AdminUpdate(request.SSectionDesc, request.STextCode, request.BtShow, request.ISortOrder, actorUserId, DateTime.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("{LogPrefix}: updated section {Id}", LogPrefix, id);
        return ConfigCatalogAdminResult.Updated(id, await TranslateAsync("section_updated", cancellationToken));
    }

    public async Task<ConfigCatalogAdminResult> DeleteSectionAsync(int id, CancellationToken cancellationToken = default)
    {
        const string LogPrefix = nameof(ConfigCatalogAdminService) + "." + nameof(DeleteSectionAsync);

        var section = await repository.GetSectionByIdAsync(id, cancellationToken);
        if (section is null)
        {
            logger.LogWarning("{LogPrefix}: section {Id} not found", LogPrefix, id);
            return ConfigCatalogAdminResult.Rejected(await TranslateAsync("not_found", cancellationToken));
        }

        if (await repository.HasConfigsForSectionAsync(id, cancellationToken))
        {
            logger.LogWarning("{LogPrefix}: rejected, section {Id} still has configs", LogPrefix, id);
            return ConfigCatalogAdminResult.Rejected(await TranslateAsync("section_has_configs", cancellationToken));
        }

        await repository.DeleteSectionAsync(section, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("{LogPrefix}: deleted section {Id}", LogPrefix, id);
        return ConfigCatalogAdminResult.Deleted(await TranslateAsync("section_deleted", cancellationToken));
    }

    public async Task<PagedResult<ConfigItemAdminDto>> ListConfigsAsync(
        int? sectionId, string? configCode, string? configName, int? configType, string? sortBy, bool sortDescending, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var skip = Math.Max(0, page - 1) * pageSize;
        var result = await repository.ListConfigsAsync(sectionId, configCode, configName, configType, sortBy, sortDescending, skip, pageSize, cancellationToken);

        if (result.Items.Count == 0)
        {
            return new PagedResult<ConfigItemAdminDto>([], result.TotalCount);
        }

        var sections = (await repository.ListSectionsAsync(cancellationToken)).ToDictionary(s => s.Id);
        var items = result.Items.Select(item => ToItemDto(item, sections)).ToList();
        return new PagedResult<ConfigItemAdminDto>(items, result.TotalCount);
    }

    public async Task<ConfigCatalogAdminResult> AddConfigAsync(ConfigItemUpsertRequest request, int actorUserId, CancellationToken cancellationToken = default)
    {
        const string LogPrefix = nameof(ConfigCatalogAdminService) + "." + nameof(AddConfigAsync);

        if (await repository.GetSectionByIdAsync(request.ISectionId, cancellationToken) is null)
        {
            logger.LogWarning("{LogPrefix}: rejected, section {SectionId} not found", LogPrefix, request.ISectionId);
            return ConfigCatalogAdminResult.Rejected(await TranslateAsync("section_not_found", cancellationToken));
        }

        if (await repository.ExistsConfigCodeAsync(request.SConfigCode, null, cancellationToken))
        {
            logger.LogWarning("{LogPrefix}: rejected, duplicate config code {ConfigCode}", LogPrefix, request.SConfigCode);
            return ConfigCatalogAdminResult.Rejected(await TranslateAsync("duplicate_key", cancellationToken));
        }

        var sortOrder = request.ISortOrder ?? (await repository.GetMaxConfigSortOrderAsync(request.ISectionId, cancellationToken) ?? 0) + 1;

        var config = new ConfigItem(
            request.ISectionId, request.SConfigCode, request.SConfigName, request.STextCode, request.IConfigType, request.BtShow, request.BtAllowUserEdit, DateTime.UtcNow,
            sortOrder, request.SDefaultValue, request.IDefaultValue, request.NDefaultValue, request.BtDefaultValue, actorUserId);
        await repository.AddConfigAsync(config, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        // Mirrors ApplyConfigDefaultsOnUserCreatedHandler's reasoning: backfilling existing users with
        // this new config's default is a side effect of the config now existing, not of this admin
        // request per se, so a hiccup here must never surface as "config creation failed" when the
        // catalog row was already committed successfully.
        try
        {
            await configUserValueService.ApplyDefaultForNewConfigAsync(config.Id, actorUserId, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "{LogPrefix}: failed to backfill existing users with the default for config {Id}", LogPrefix, config.Id);
        }

        logger.LogInformation("{LogPrefix}: created config {Id}", LogPrefix, config.Id);
        return ConfigCatalogAdminResult.Created(config.Id, await TranslateAsync("config_created", cancellationToken));
    }

    public async Task<ConfigCatalogAdminResult> UpdateConfigAsync(int id, ConfigItemUpsertRequest request, int actorUserId, CancellationToken cancellationToken = default)
    {
        const string LogPrefix = nameof(ConfigCatalogAdminService) + "." + nameof(UpdateConfigAsync);

        var config = await repository.GetConfigByIdAsync(id, cancellationToken);
        if (config is null)
        {
            logger.LogWarning("{LogPrefix}: config {Id} not found", LogPrefix, id);
            return ConfigCatalogAdminResult.Rejected(await TranslateAsync("not_found", cancellationToken));
        }

        if (await repository.GetSectionByIdAsync(request.ISectionId, cancellationToken) is null)
        {
            logger.LogWarning("{LogPrefix}: rejected, section {SectionId} not found", LogPrefix, request.ISectionId);
            return ConfigCatalogAdminResult.Rejected(await TranslateAsync("section_not_found", cancellationToken));
        }

        if (await repository.ExistsConfigCodeAsync(request.SConfigCode, id, cancellationToken))
        {
            logger.LogWarning("{LogPrefix}: rejected, duplicate config code {ConfigCode}", LogPrefix, request.SConfigCode);
            return ConfigCatalogAdminResult.Rejected(await TranslateAsync("duplicate_key", cancellationToken));
        }

        config.AdminUpdate(
            request.ISectionId, request.SConfigCode, request.SConfigName, request.STextCode, request.IConfigType, request.BtShow, request.BtAllowUserEdit, request.ISortOrder,
            request.SDefaultValue, request.IDefaultValue, request.NDefaultValue, request.BtDefaultValue, actorUserId, DateTime.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("{LogPrefix}: updated config {Id}", LogPrefix, id);
        return ConfigCatalogAdminResult.Updated(id, await TranslateAsync("config_updated", cancellationToken));
    }

    public async Task<ConfigCatalogAdminResult> DeleteConfigAsync(int id, CancellationToken cancellationToken = default)
    {
        const string LogPrefix = nameof(ConfigCatalogAdminService) + "." + nameof(DeleteConfigAsync);

        var config = await repository.GetConfigByIdAsync(id, cancellationToken);
        if (config is null)
        {
            logger.LogWarning("{LogPrefix}: config {Id} not found", LogPrefix, id);
            return ConfigCatalogAdminResult.Rejected(await TranslateAsync("not_found", cancellationToken));
        }

        await repository.DeleteConfigAsync(config, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("{LogPrefix}: deleted config {Id}", LogPrefix, id);
        return ConfigCatalogAdminResult.Deleted(await TranslateAsync("config_deleted", cancellationToken));
    }

    private static ConfigSectionAdminDto ToSectionDto(ConfigSection section) =>
        new(section.Id, section.SectionDesc, section.TextCode, section.Show, section.SortOrder);

    private static ConfigItemAdminDto ToItemDto(ConfigItem item, IReadOnlyDictionary<int, ConfigSection> sections)
    {
        var sectionDesc = sections.TryGetValue(item.SectionId, out var section) ? section.SectionDesc : $"#{item.SectionId}";
        return new ConfigItemAdminDto(
            item.Id, item.SectionId, sectionDesc, item.ConfigCode, item.ConfigName, item.TextCode, item.ConfigType, item.Show, item.AllowUserEdit, item.SortOrder,
            item.DefaultStringValue, item.DefaultIntValue, item.DefaultDecimalValue, item.DefaultBoolValue);
    }
}
