using Microsoft.EntityFrameworkCore;
using OCRWeb.Identity.Domain.Repositories;

namespace OCRWeb.Identity.Infrastructure.Repositories;

public class SecurityUserRuleItemRepository(UserDbContext db) : ISecurityUserRuleItemRepository
{
    public Task<bool> HasAnyForUserAsync(int userId, CancellationToken ct = default) =>
        db.SecurityUserRuleItems.AnyAsync(x => x.UserId == userId, ct);

    public Task<bool> HasAnyForRuleItemAsync(int ruleItemId, CancellationToken ct = default) =>
        db.SecurityUserRuleItems.AnyAsync(x => x.RuleItemId == ruleItemId, ct);
}
