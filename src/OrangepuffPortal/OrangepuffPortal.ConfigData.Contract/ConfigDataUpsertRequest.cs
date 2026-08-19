namespace OrangepuffPortal.ConfigData.Contract;

/// <summary>
/// Payload for both create and update admin operations on a ConfigData row.
/// Field names use the DB column-name prefix convention so the gateway can pass them through without renaming.
/// </summary>
public sealed record ConfigDataUpsertRequest(string SKey, string? SValue, bool? BAllowEditByScreen, string? SDescription);
