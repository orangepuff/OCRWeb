namespace OrangepuffPortal.Bff.Infrastructure.IdentityGateway
{
    public record AddSecurityRuleItemResult(bool Success, int? RuleItemId, string? RejectionReason, string? SuccessMessage);
}
