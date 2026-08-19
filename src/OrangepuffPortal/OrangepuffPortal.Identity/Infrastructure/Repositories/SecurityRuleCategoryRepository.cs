using Microsoft.EntityFrameworkCore;
using OrangepuffPortal.Identity.Domain.Entity;
using OrangepuffPortal.Identity.Domain.Repositories;

namespace OrangepuffPortal.Identity.Infrastructure.Repositories;

public class SecurityRuleCategoryRepository(UserDbContext db) : ISecurityRuleCategoryRepository
{
    public Task<SecurityRuleCategory?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.SecurityRuleCategories.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<SecurityRuleCategory?> GetByDescAsync(string categoryDesc, CancellationToken ct = default) =>
        db.SecurityRuleCategories.FirstOrDefaultAsync(x => x.CategoryDesc == categoryDesc, ct);

    public async Task<IReadOnlyList<SecurityRuleCategory>> GetAllAsync(CancellationToken ct = default) =>
        await db.SecurityRuleCategories.OrderBy(x => x.CategoryDesc).ToListAsync(ct);

    public Task<bool> HasItemsAsync(int categoryId, CancellationToken ct = default) =>
        db.SecurityRuleItems.AnyAsync(x => x.CategoryId == categoryId, ct);

    public async Task AddAsync(SecurityRuleCategory category, CancellationToken ct = default) => await db.SecurityRuleCategories.AddAsync(category, ct);

    public Task DeleteAsync(SecurityRuleCategory category, CancellationToken ct = default)
    {
        db.SecurityRuleCategories.Remove(category);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
