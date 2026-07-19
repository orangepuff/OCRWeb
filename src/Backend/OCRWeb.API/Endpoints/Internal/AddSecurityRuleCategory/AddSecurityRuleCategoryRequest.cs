namespace OCRWeb.API.Endpoints.Internal.AddSecurityRuleCategory
{
    public class AddSecurityRuleCategoryRequest
    {
        public required string CategoryDesc { get; set; }
        public string? TextCode { get; set; }
    }
}
