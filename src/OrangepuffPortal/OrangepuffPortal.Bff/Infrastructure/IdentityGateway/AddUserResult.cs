namespace OrangepuffPortal.Bff.Infrastructure.IdentityGateway
{
    public record AddUserResult(bool Success, int? UserId, string? RejectionReason, string? SuccessMessage);
}
