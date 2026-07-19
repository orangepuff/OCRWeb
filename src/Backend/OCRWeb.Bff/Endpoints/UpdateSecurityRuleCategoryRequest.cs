namespace OCRWeb.Bff.Endpoints
{
    public record UpdateSecurityRuleCategoryRequest(string CategoryDesc, string? TextCode, bool Hidden);
}
