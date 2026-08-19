namespace OrangepuffPortal.Bff.Endpoints.SecurityRuleItemAdminEndpoints
{
    public record UpdateSecurityRuleItemRequest(int CategoryId, string Description, int RuleType, string? TextCode, int? SortOrder, bool Hidden);
}
