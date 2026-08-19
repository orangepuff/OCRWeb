namespace OrangepuffPortal.Bff.Infrastructure.IdentityGateway
{
    public record PasswordSignInResult(bool Success, int? UserId, string? Email, string? DisplayName, string? CultureCode, string? RejectionReason);
}
