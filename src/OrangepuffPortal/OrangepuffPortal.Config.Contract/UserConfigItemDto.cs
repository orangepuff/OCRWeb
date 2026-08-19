namespace OrangepuffPortal.Config.Contract;

/// <summary>
/// One config item within a <see cref="UserConfigSectionDto"/>, catalog metadata merged with a
/// specific user's current value (all four value fields null if they've never set one).
/// </summary>
public sealed record UserConfigItemDto(
    string SConfigCode,
    string SConfigName,
    string STextCode,
    ConfigValueType ConfigType,
    bool BtAllowUserEdit,
    int? ISortOrder,
    string? SConfigValue,
    int? IConfigValue,
    decimal? NConfigValue,
    bool? BtConfigValue);
