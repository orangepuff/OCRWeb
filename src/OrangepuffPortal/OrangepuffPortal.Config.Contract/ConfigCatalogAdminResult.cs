namespace OrangepuffPortal.Config.Contract;

public sealed record ConfigCatalogAdminResult(bool Success, int? Id, string? RejectionReason, string? SuccessMessage)
{
    public static ConfigCatalogAdminResult Created(int id, string successMessage) => new(true, id, null, successMessage);

    public static ConfigCatalogAdminResult Updated(int id, string successMessage) => new(true, id, null, successMessage);

    public static ConfigCatalogAdminResult Deleted(string successMessage) => new(true, null, null, successMessage);

    public static ConfigCatalogAdminResult Rejected(string reason) => new(false, null, reason, null);
}
