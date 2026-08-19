namespace OrangepuffPortal.ConfigText.Contract;

/// <summary>Fields editable from the admin CRUD screen — used for both add and edit.</summary>
public sealed record ConfigTextAdminUpsertRequest(
    string SModule,
    string STextCode,
    string SCultureCode,
    string STextType,
    string SText,
    string? SNote);
