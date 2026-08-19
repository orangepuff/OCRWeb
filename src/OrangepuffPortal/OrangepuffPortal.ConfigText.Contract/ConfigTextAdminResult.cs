namespace OrangepuffPortal.ConfigText.Contract;

public sealed record ConfigTextAdminResult(bool Success, int? Id, string? RejectionReason, string? SuccessMessage)
{
    public static ConfigTextAdminResult Created(int id, string successMessage) => new(true, id, null, successMessage);

    public static ConfigTextAdminResult Updated(int id, string successMessage) => new(true, id, null, successMessage);

    public static ConfigTextAdminResult Deleted(string successMessage) => new(true, null, null, successMessage);

    public static ConfigTextAdminResult Rejected(string reason) => new(false, null, reason, null);
}
