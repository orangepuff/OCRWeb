namespace OCRWeb.Bff.Infrastructure.IdentityApiClient
{
    public record AddUserResult(bool Success, int? UserId, string? RejectionReason);
}
