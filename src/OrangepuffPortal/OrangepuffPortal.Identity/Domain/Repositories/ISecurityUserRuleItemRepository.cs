using OrangepuffPortal.Identity.Domain.Entity;

namespace OrangepuffPortal.Identity.Domain.Repositories;

/// <summary>
/// Queries against [identity].[SecurityUserRuleItems].
/// </summary>
public interface ISecurityUserRuleItemRepository
{
    Task<bool> HasAnyForUserAsync(int userId, CancellationToken ct = default);
    Task<bool> HasAnyForRuleItemAsync(int ruleItemId, CancellationToken ct = default);
    Task<IReadOnlyList<SecurityUserRuleItem>> GetForUserAsync(int userId, CancellationToken ct = default);
}
