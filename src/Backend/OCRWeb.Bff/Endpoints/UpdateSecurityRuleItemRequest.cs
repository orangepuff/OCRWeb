namespace OCRWeb.Bff.Endpoints
{
    public record UpdateSecurityRuleItemRequest(int CategoryId, string Description, int RuleType, string? TextCode, int? SortOrder, bool Hidden);
}
