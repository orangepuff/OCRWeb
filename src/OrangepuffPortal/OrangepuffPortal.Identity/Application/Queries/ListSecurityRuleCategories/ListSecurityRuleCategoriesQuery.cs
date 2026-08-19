using MediatR;
using OrangepuffPortal.Identity.Contract;

namespace OrangepuffPortal.Identity.Application.Queries.ListSecurityRuleCategories;

/// <summary>List all security rule categories.</summary>
public record ListSecurityRuleCategoriesQuery : IRequest<IReadOnlyList<SecurityRuleCategoryListItemDto>>;
