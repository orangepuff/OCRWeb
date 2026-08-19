namespace OrangepuffPortal.Config.Contract;

/// <summary>Fields editable from the admin CRUD screen — used for both add and edit.</summary>
public sealed record ConfigItemUpsertRequest(
    int ISectionId,
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
