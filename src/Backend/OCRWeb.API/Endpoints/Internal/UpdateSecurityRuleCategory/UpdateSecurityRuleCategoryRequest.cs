namespace OCRWeb.API.Endpoints.Internal.UpdateSecurityRuleCategory
{
    public class UpdateSecurityRuleCategoryRequest
    {
        public int Id { get; set; }
        public required string CategoryDesc { get; set; }
        public string? TextCode { get; set; }
        public bool Hidden { get; set; }
    }
}
