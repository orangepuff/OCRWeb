namespace OCRWeb.Bff.Infrastructure.IdentityApiClient
{
    public record GoogleProvisionResult(bool Success, int? UserId, string? RejectionReason);
}
