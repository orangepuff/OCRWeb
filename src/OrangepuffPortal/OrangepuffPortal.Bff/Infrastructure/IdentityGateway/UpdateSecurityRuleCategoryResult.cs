namespace OrangepuffPortal.Bff.Infrastructure.IdentityGateway
{
    public record UpdateSecurityRuleCategoryResult(bool Success, string? RejectionReason, string? SuccessMessage);
}
