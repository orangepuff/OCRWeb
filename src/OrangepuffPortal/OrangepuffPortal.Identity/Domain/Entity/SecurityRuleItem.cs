using OrangepuffPortal.Identity.Domain.Enums;
using OrangepuffPortal.Shared.Domain;

namespace OrangepuffPortal.Identity.Domain.Entity;

/// <summary>
/// A single permission code within a <see cref="SecurityRuleCategory"/>. Maps to [identity].[SecurityRuleItems].
/// <see cref="RuleType"/> determines how a <see cref="SecurityUserRuleItem"/> assignment's value is interpreted.
/// </summary>
public class SecurityRuleItem : AuditableEntity
{
    public int Id { get; private set; }
    public int CategoryId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public RuleType RuleType { get; private set; }
    public int? SortOrder { get; private set; }
    public string? TextCode { get; private set; }
    public bool Hidden { get; private set; }

    private SecurityRuleItem() { } // EF

    public SecurityRuleItem(int categoryId, string code, string description, RuleType ruleType, string? textCode, int? sortOrder, int userId, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Code is required.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description is required.", nameof(description));
        }

        CategoryId = categoryId;
        Code = code.Trim();
        Description = description.Trim();
        RuleType = ruleType;
        TextCode = string.IsNullOrWhiteSpace(textCode) ? null : textCode.Trim();
        SortOrder = sortOrder;
        MarkInserted(userId, utcNow);
    }

    public void SetSortOrder(int? sortOrder, int userId, DateTime utcNow)
    {
        SortOrder = sortOrder;
        MarkUpdated(userId, utcNow);
    }

    public void SetHidden(bool hidden, int userId, DateTime utcNow)
    {
        Hidden = hidden;
        MarkUpdated(userId, utcNow);
    }

    public void UpdateDetails(int categoryId, string description, RuleType ruleType, string? textCode, int? sortOrder, int userId, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description is required.", nameof(description));
        }

        CategoryId = categoryId;
        Description = description.Trim();
        RuleType = ruleType;
        TextCode = string.IsNullOrWhiteSpace(textCode) ? null : textCode.Trim();
        SortOrder = sortOrder;
        MarkUpdated(userId, utcNow);
    }
}
