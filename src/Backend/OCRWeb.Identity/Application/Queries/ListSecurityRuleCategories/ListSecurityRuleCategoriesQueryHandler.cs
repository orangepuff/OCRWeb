using MediatR;
using OCRWeb.Identity.Contract;
using OCRWeb.Identity.Domain.Repositories;

namespace OCRWeb.Identity.Application.Queries.ListSecurityRuleCategories;

public class ListSecurityRuleCategoriesQueryHandler(ISecurityRuleCategoryRepository repository)
    : IRequestHandler<ListSecurityRuleCategoriesQuery, IReadOnlyList<SecurityRuleCategoryListItemDto>>
{
    public async Task<IReadOnlyList<SecurityRuleCategoryListItemDto>> Handle(ListSecurityRuleCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await repository.GetAllAsync(cancellationToken);
        return categories
            .Select(c => new SecurityRuleCategoryListItemDto(c.Id, c.CategoryDesc, c.TextCode, c.Hidden))
            .ToList();
    }
}
