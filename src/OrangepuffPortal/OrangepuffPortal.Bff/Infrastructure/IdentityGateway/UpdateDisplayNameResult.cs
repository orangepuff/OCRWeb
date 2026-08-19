namespace OrangepuffPortal.Bff.Infrastructure.IdentityGateway
{
    public record UpdateDisplayNameResult(bool Success, string? RejectionReason, string? SuccessMessage);
}
