using MediatR;
using OrangepuffPortal.Identity.Contract;
using OrangepuffPortal.Identity.Domain.Repositories;

namespace OrangepuffPortal.Identity.Application.Queries.ListSecurityRuleItems;

public class ListSecurityRuleItemsQueryHandler(ISecurityRuleItemRepository repository)
    : IRequestHandler<ListSecurityRuleItemsQuery, IReadOnlyList<SecurityRuleItemListItemDto>>
{
    public async Task<IReadOnlyList<SecurityRuleItemListItemDto>> Handle(ListSecurityRuleItemsQuery request, CancellationToken cancellationToken)
    {
        var items = await repository.GetAllAsync(request.CategoryId, cancellationToken);
        return items
            .Select(i => new SecurityRuleItemListItemDto(i.Id, i.CategoryId, i.Code, i.Description, i.RuleType.ToString(), i.SortOrder, i.TextCode, i.Hidden))
            .ToList();
    }
}
