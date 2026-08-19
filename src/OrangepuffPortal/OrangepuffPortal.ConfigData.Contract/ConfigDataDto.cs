namespace OrangepuffPortal.ConfigData.Contract;

/// <summary>
/// Read-only projection of a single ConfigData row, safe to pass across module boundaries.
/// </summary>
public sealed record ConfigDataDto(int Id, string Key, string? Value, bool? AllowEditByScreen, string? Description);
