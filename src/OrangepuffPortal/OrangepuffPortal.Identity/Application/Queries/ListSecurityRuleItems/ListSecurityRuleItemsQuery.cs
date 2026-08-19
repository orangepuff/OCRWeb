using MediatR;
using OrangepuffPortal.Identity.Contract;

namespace OrangepuffPortal.Identity.Application.Queries.ListSecurityRuleItems;

/// <summary>List security rule items, optionally filtered to one category.</summary>
public record ListSecurityRuleItemsQuery(int? CategoryId) : IRequest<IReadOnlyList<SecurityRuleItemListItemDto>>;
