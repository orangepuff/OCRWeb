using OrangepuffPortal.Shared.Domain;

namespace OrangepuffPortal.Identity.Domain.Entity;

/// <summary>
/// Groups related <see cref="SecurityRuleItem"/> permission codes. Maps to [identity].[SecurityRuleCategory].
/// </summary>
public class SecurityRuleCategory : AuditableEntity
{
    public int Id { get; private set; }
    public string CategoryDesc { get; private set; } = string.Empty;
    public string? TextCode { get; private set; }
    public bool Hidden { get; private set; }

    private SecurityRuleCategory() { } // EF

    public SecurityRuleCategory(string categoryDesc, string? textCode, int userId, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(categoryDesc))
        {
            throw new ArgumentException("CategoryDesc is required.", nameof(categoryDesc));
        }

        CategoryDesc = categoryDesc.Trim();
        TextCode = string.IsNullOrWhiteSpace(textCode) ? null : textCode.Trim();
        MarkInserted(userId, utcNow);
    }

    public void SetHidden(bool hidden, int userId, DateTime utcNow)
    {
        Hidden = hidden;
        MarkUpdated(userId, utcNow);
    }

    public void UpdateDetails(string categoryDesc, string? textCode, int userId, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(categoryDesc))
        {
            throw new ArgumentException("CategoryDesc is required.", nameof(categoryDesc));
        }

        CategoryDesc = categoryDesc.Trim();
        TextCode = string.IsNullOrWhiteSpace(textCode) ? null : textCode.Trim();
        MarkUpdated(userId, utcNow);
    }
}
