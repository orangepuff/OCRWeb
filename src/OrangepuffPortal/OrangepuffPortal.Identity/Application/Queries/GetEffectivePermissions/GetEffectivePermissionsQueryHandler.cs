using MediatR;
using OrangepuffPortal.Identity.Contract;
using OrangepuffPortal.Identity.Domain.Repositories;

namespace OrangepuffPortal.Identity.Application.Queries.GetEffectivePermissions;

public class GetEffectivePermissionsQueryHandler(
    IUserRepository userRepository
    , ISecurityUserRuleItemRepository userRuleItemRepository
    , ISecurityRuleItemRepository ruleItemRepository) : IRequestHandler<GetEffectivePermissionsQuery, IReadOnlyList<EffectivePermissionDto>>
{
    public async Task<IReadOnlyList<EffectivePermissionDto>> Handle(GetEffectivePermissionsQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return [];
        }

        var effectiveUserId = user.ParentId ?? user.Id;
        var assignments = await userRuleItemRepository.GetForUserAsync(effectiveUserId, cancellationToken);
        if (assignments.Count == 0)
        {
            return [];
        }

        var ruleItems = await ruleItemRepository.GetByIdsAsync(assignments.Select(a => a.RuleItemId).ToList(), cancellationToken);
        var ruleItemsById = ruleItems.ToDictionary(r => r.Id);

        return assignments
            .Where(a => ruleItemsById.ContainsKey(a.RuleItemId))
            .Select(a =>
            {
                var rule = ruleItemsById[a.RuleItemId];
                return new EffectivePermissionDto(rule.Code, rule.Description, rule.RuleType.ToString(), a.Allowed, a.AllowedDecimal);
            })
            .ToList();
    }
}
