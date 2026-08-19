namespace OrangepuffPortal.Bff.Infrastructure.IdentityGateway
{
    public record DeleteSecurityRuleCategoryResult(bool Success, string? RejectionReason, string? SuccessMessage);
}
