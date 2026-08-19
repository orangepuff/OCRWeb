namespace OrangepuffPortal.Config.Contract;

/// <summary>A visible section and its visible configs, for one user, as returned by <see cref="Interfaces.IConfigUserValueService.GetSectionsForUserAsync"/>.</summary>
public sealed record UserConfigSectionDto(
    string SSectionDesc,
    string STextCode,
    int? ISortOrder,
    IReadOnlyCollection<UserConfigItemDto> Configs);
