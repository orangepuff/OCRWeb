namespace OrangepuffPortal.Config.Contract;

/// <summary>One row of [config].[Configs] for the admin grid, with its parent section's description for display.</summary>
public sealed record ConfigItemAdminDto(
    int Id,
    int ISectionId,
    string SSectionDesc,
    string SConfigCode,
    string SConfigName,
    string STextCode,
    int IConfigType,
    bool BtShow,
    bool BtAllowUserEdit,
    int? ISortOrder,
    string? SDefaultValue,
    int? IDefaultValue,
    decimal? NDefaultValue,
    bool? BtDefaultValue);
