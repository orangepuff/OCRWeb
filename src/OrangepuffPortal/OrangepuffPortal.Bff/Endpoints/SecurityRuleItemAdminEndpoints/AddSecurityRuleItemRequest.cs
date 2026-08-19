namespace OrangepuffPortal.Bff.Endpoints.SecurityRuleItemAdminEndpoints
{
    public record AddSecurityRuleItemRequest(int CategoryId, string Code, string Description, int RuleType, string? TextCode, int? SortOrder);
}
