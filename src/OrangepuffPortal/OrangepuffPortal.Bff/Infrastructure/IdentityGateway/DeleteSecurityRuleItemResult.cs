namespace OrangepuffPortal.Bff.Infrastructure.IdentityGateway
{
    public record DeleteSecurityRuleItemResult(bool Success, string? RejectionReason, string? SuccessMessage);
}
