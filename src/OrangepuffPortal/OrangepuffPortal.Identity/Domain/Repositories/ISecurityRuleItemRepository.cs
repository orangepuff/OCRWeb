using OrangepuffPortal.Identity.Domain.Entity;

namespace OrangepuffPortal.Identity.Domain.Repositories;

public interface ISecurityRuleItemRepository
{
    Task<SecurityRuleItem?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<SecurityRuleItem?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<SecurityRuleItem>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default);
    Task<IReadOnlyList<SecurityRuleItem>> GetAllAsync(int? categoryId, CancellationToken ct = default);
    Task AddAsync(SecurityRuleItem item, CancellationToken ct = default);
    Task DeleteAsync(SecurityRuleItem item, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
