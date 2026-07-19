namespace OCRWeb.Identity.Domain.Repositories;

/// <summary>
/// Constraint-check queries against [identity].[SecurityUserRuleItems].
/// No CRUD here yet — only what AddUser/UpdateUser/DeleteSecurityRuleItem need to validate.
/// </summary>
public interface ISecurityUserRuleItemRepository
{
    Task<bool> HasAnyForUserAsync(int userId, CancellationToken ct = default);
    Task<bool> HasAnyForRuleItemAsync(int ruleItemId, CancellationToken ct = default);
}
