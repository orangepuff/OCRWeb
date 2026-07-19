namespace OCRWeb.Bff.Infrastructure.IdentityApiClient
{
    public record AddSecurityRuleItemResult(bool Success, int? RuleItemId, string? RejectionReason);
}
