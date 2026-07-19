namespace OCRWeb.API.Endpoints.Internal.AddSecurityRuleItem
{
    public record AddSecurityRuleItemResponse(bool Success, int? RuleItemId, string? RejectionReason);
}
