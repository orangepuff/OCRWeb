using Microsoft.EntityFrameworkCore;
using OrangepuffPortal.Identity.Domain.Entity;
using OrangepuffPortal.Identity.Domain.Repositories;

namespace OrangepuffPortal.Identity.Infrastructure.Repositories;

public class SecurityUserRuleItemRepository(UserDbContext db) : ISecurityUserRuleItemRepository
{
    public Task<bool> HasAnyForUserAsync(int userId, CancellationToken ct = default) =>
        db.SecurityUserRuleItems.AnyAsync(x => x.UserId == userId, ct);

    public Task<bool> HasAnyForRuleItemAsync(int ruleItemId, CancellationToken ct = default) =>
        db.SecurityUserRuleItems.AnyAsync(x => x.RuleItemId == ruleItemId, ct);

    public async Task<IReadOnlyList<SecurityUserRuleItem>> GetForUserAsync(int userId, CancellationToken ct = default) =>
        await db.SecurityUserRuleItems.Where(x => x.UserId == userId).ToListAsync(ct);
}
