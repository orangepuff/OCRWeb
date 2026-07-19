using OCRWeb.Identity.Domain.Enums;

namespace OCRWeb.API.Endpoints.Internal.AddSecurityRuleItem
{
    public class AddSecurityRuleItemRequest
    {
        public int CategoryId { get; set; }
        public required string Code { get; set; }
        public required string Description { get; set; }
        public RuleType RuleType { get; set; }
        public string? TextCode { get; set; }
        public int? SortOrder { get; set; }
    }
}
