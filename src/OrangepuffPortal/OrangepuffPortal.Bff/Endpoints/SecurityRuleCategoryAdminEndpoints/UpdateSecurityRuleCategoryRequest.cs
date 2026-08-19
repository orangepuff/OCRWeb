namespace OrangepuffPortal.Bff.Endpoints.SecurityRuleCategoryAdminEndpoints
{
    public record UpdateSecurityRuleCategoryRequest(string CategoryDesc, string? TextCode, bool Hidden);
}
