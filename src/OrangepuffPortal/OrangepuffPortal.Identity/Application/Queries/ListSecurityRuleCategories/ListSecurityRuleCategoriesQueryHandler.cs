using MediatR;
using OrangepuffPortal.Identity.Contract;
using OrangepuffPortal.Identity.Domain.Repositories;

namespace OrangepuffPortal.Identity.Application.Queries.ListSecurityRuleCategories;

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
