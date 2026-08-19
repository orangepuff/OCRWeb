namespace OrangepuffPortal.ConfigText.Contract;

/// <summary>
/// A single resolved text entry: the row matching the requested culture, or the "*" fallback row when no culture-specific row exists.
/// </summary>
public sealed record ConfigTextEntryDto(
    string SModule,
    string STextCode,
    string STextType,
    string SText);
