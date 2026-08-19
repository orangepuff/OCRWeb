using OrangepuffPortal.Shared.Domain;

namespace OrangepuffPortal.Identity.Domain.Entity;

/// <summary>
/// Assigns a <see cref="SecurityRuleItem"/> to a <see cref="User"/>. Maps to [identity].[SecurityUserRuleItems].
/// Only valid for users with no ParentId (see <see cref="User.ParentId"/>); a user inheriting from a
/// template user has no rows here and resolves permissions from the template user instead.
/// </summary>
public class SecurityUserRuleItem : AuditableEntity
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public int RuleItemId { get; private set; }

    /// <summary>0/1 flag when the rule is RuleType.Boolean, or the numeric limit when RuleType.Integer.</summary>
    public int? Allowed { get; private set; }

    /// <summary>The numeric limit when the rule is RuleType.Decimal.</summary>
    public decimal? AllowedDecimal { get; private set; }

    private SecurityUserRuleItem() { } // EF

    public SecurityUserRuleItem(int userId, int ruleItemId, int? allowed, decimal? allowedDecimal, int insertingUserId, DateTime utcNow)
    {
        UserId = userId;
        RuleItemId = ruleItemId;
        Allowed = allowed;
        AllowedDecimal = allowedDecimal;
        MarkInserted(insertingUserId, utcNow);
    }

    public void SetValue(int? allowed, decimal? allowedDecimal, int userId, DateTime utcNow)
    {
        Allowed = allowed;
        AllowedDecimal = allowedDecimal;
        MarkUpdated(userId, utcNow);
    }
}
