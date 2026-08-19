namespace OrangepuffPortal.Config.Contract;

/// <summary>A resolved user value for one config. Only the field matching <see cref="ConfigType"/> is set.</summary>
public sealed record ConfigUserValueDto(
    string SConfigCode,
    ConfigValueType ConfigType,
    string? SConfigValue,
    int? IConfigValue,
    decimal? NConfigValue,
    bool? BtConfigValue);
