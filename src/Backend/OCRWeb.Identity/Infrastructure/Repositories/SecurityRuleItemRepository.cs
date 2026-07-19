using Microsoft.EntityFrameworkCore;
using OCRWeb.Identity.Domain.Entity;
using OCRWeb.Identity.Domain.Repositories;

namespace OCRWeb.Identity.Infrastructure.Repositories;

public class SecurityRuleItemRepository(UserDbContext db) : ISecurityRuleItemRepository
{
    public Task<SecurityRuleItem?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.SecurityRuleItems.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<SecurityRuleItem?> GetByCodeAsync(string code, CancellationToken ct = default) =>
        db.SecurityRuleItems.FirstOrDefaultAsync(x => x.Code == code, ct);

    public async Task AddAsync(SecurityRuleItem item, CancellationToken ct = default) => await db.SecurityRuleItems.AddAsync(item, ct);

    public Task DeleteAsync(SecurityRuleItem item, CancellationToken ct = default)
    {
        db.SecurityRuleItems.Remove(item);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
