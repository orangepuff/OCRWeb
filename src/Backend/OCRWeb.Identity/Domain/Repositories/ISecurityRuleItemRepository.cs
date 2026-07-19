using OCRWeb.Identity.Domain.Entity;

namespace OCRWeb.Identity.Domain.Repositories;

public interface ISecurityRuleItemRepository
{
    Task<SecurityRuleItem?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<SecurityRuleItem?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task AddAsync(SecurityRuleItem item, CancellationToken ct = default);
    Task DeleteAsync(SecurityRuleItem item, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
