namespace OrangepuffPortal.ConfigText.Contract;

/// <summary>
/// One full row of [configtext].[ConfigTextDefinition] for the admin grid — unlike <see cref="ConfigTextEntryDto"/>,
/// this includes the row id and audit timestamps needed to edit/delete it.
/// </summary>
public sealed record ConfigTextAdminDto(
    int Id,
    string SModule,
    string STextCode,
    string SCultureCode,
    string STextType,
    string SText,
    string? SNote,
    DateTime? DtInsertedTime,
    DateTime? DtUpdatedTime);
