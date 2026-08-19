namespace OrangepuffPortal.Bff.Infrastructure.IdentityGateway
{
    public record SetUserPasswordResult(bool Success, string? RejectionReason, string? SuccessMessage);
}
