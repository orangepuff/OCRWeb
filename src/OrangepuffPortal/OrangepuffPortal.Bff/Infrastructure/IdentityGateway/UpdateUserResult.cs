namespace OrangepuffPortal.Bff.Infrastructure.IdentityGateway
{
    public record UpdateUserResult(bool Success, string? RejectionReason, string? SuccessMessage);
}
