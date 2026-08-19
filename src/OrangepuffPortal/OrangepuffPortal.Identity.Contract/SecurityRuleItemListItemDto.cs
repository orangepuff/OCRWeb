namespace OrangepuffPortal.Identity.Contract;

public record SecurityRuleItemListItemDto(int Id, int CategoryId, string Code, string Description, string RuleType, int? SortOrder, string? TextCode, bool Hidden);
