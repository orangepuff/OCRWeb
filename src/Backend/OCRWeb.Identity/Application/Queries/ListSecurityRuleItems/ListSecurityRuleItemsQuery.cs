using MediatR;
using OCRWeb.Identity.Contract;

namespace OCRWeb.Identity.Application.Queries.ListSecurityRuleItems;

/// <summary>List security rule items, optionally filtered to one category.</summary>
public record ListSecurityRuleItemsQuery(int? CategoryId) : IRequest<IReadOnlyList<SecurityRuleItemListItemDto>>;
