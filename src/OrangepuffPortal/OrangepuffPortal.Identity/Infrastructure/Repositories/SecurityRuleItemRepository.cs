using Microsoft.EntityFrameworkCore;
using OrangepuffPortal.Identity.Domain.Entity;
using OrangepuffPortal.Identity.Domain.Repositories;

namespace OrangepuffPortal.Identity.Infrastructure.Repositories;

public class SecurityRuleItemRepository(UserDbContext db) : ISecurityRuleItemRepository
{
    public Task<SecurityRuleItem?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.SecurityRuleItems.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<SecurityRuleItem?> GetByCodeAsync(string code, CancellationToken ct = default) =>
        db.SecurityRuleItems.FirstOrDefaultAsync(x => x.Code == code, ct);

    public async Task<IReadOnlyList<SecurityRuleItem>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default) =>
        await db.SecurityRuleItems.Where(x => ids.Contains(x.Id)).ToListAsync(ct);

    public async Task<IReadOnlyList<SecurityRuleItem>> GetAllAsync(int? categoryId, CancellationToken ct = default) =>
        await db.SecurityRuleItems
            .Where(x => categoryId == null || x.CategoryId == categoryId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Code)
            .ToListAsync(ct);

    public async Task AddAsync(SecurityRuleItem item, CancellationToken ct = default) => await db.SecurityRuleItems.AddAsync(item, ct);

    public Task DeleteAsync(SecurityRuleItem item, CancellationToken ct = default)
    {
        db.SecurityRuleItems.Remove(item);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
