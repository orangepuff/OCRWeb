namespace OrangepuffPortal.Bff.Infrastructure.IdentityGateway
{
    public record DeleteUserResult(bool Success, string? RejectionReason, string? SuccessMessage);
}
