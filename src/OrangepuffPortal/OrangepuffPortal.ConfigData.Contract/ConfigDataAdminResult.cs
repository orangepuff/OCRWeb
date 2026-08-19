namespace OrangepuffPortal.ConfigData.Contract;

public sealed record ConfigDataAdminResult(bool Success, int? Id, string? RejectionReason, string? SuccessMessage)
{
    public static ConfigDataAdminResult Created(int id, string successMessage) => new(true, id, null, successMessage);

    public static ConfigDataAdminResult Updated(int id, string successMessage) => new(true, id, null, successMessage);

    public static ConfigDataAdminResult Deleted(string successMessage) => new(true, null, null, successMessage);

    public static ConfigDataAdminResult Rejected(string reason) => new(false, null, reason, null);
}
