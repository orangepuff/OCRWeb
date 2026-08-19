namespace OrangepuffPortal.Bff.Infrastructure.IdentityGateway
{
    public record UpdateSecurityRuleItemResult(bool Success, string? RejectionReason, string? SuccessMessage);
}
