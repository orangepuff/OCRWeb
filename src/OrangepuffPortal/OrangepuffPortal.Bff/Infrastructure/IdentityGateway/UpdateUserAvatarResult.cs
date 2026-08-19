namespace OrangepuffPortal.Bff.Infrastructure.IdentityGateway
{
    public record UpdateUserAvatarResult(bool Success, string? RejectionReason, string? SuccessMessage);
}
