namespace OCRWeb.API.Endpoints.Internal.AddSecurityRuleCategory
{
    public record AddSecurityRuleCategoryResponse(bool Success, int? CategoryId, string? RejectionReason);
}
