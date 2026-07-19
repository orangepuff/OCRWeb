using MediatR;
using OCRWeb.Identity.Contract;

namespace OCRWeb.Identity.Application.Queries.ListSecurityRuleCategories;

/// <summary>List all security rule categories.</summary>
public record ListSecurityRuleCategoriesQuery : IRequest<IReadOnlyList<SecurityRuleCategoryListItemDto>>;
