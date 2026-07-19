using OCRWeb.Identity.Domain.Enums;

namespace OCRWeb.API.Endpoints.Internal.UpdateSecurityRuleItem
{
    public class UpdateSecurityRuleItemRequest
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public required string Description { get; set; }
        public RuleType RuleType { get; set; }
        public string? TextCode { get; set; }
        public int? SortOrder { get; set; }
        public bool Hidden { get; set; }
    }
}
