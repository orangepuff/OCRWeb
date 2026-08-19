using OrangepuffPortal.Identity.Domain.Entity;

namespace OrangepuffPortal.Identity.Domain.Repositories;

public interface ISecurityRuleCategoryRepository
{
    Task<SecurityRuleCategory?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<SecurityRuleCategory?> GetByDescAsync(string categoryDesc, CancellationToken ct = default);
    Task<IReadOnlyList<SecurityRuleCategory>> GetAllAsync(CancellationToken ct = default);
    Task<bool> HasItemsAsync(int categoryId, CancellationToken ct = default);
    Task AddAsync(SecurityRuleCategory category, CancellationToken ct = default);
    Task DeleteAsync(SecurityRuleCategory category, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
