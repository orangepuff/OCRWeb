namespace OrangepuffPortal.Config.Contract;

/// <summary>
/// New value for a config, supplied by the caller. Exactly one field should be set, matching the
/// target config's <see cref="ConfigValueType"/> — <see cref="Interfaces.IConfigUserValueService"/>
/// does not validate which one.
/// </summary>
public sealed record ConfigValueInput(
    string? SConfigValue = null,
    int? IConfigValue = null,
    decimal? NConfigValue = null,
    bool? BtConfigValue = null);
