namespace OrangepuffPortal.Bff.Infrastructure.IdentityGateway
{
    public record ChangeOwnPasswordResult(bool Success, string? RejectionReason, string? SuccessMessage);
}
