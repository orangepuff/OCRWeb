namespace OrangepuffPortal.Identity.Domain.Enums;

/// <summary>
/// How a <see cref="Entity.SecurityRuleItem"/> assignment's value is interpreted.
/// Boolean uses <see cref="Entity.SecurityUserRuleItem.Allowed"/> as a 0/1 flag.
/// Integer uses <see cref="Entity.SecurityUserRuleItem.Allowed"/> as a numeric limit.
/// Decimal uses <see cref="Entity.SecurityUserRuleItem.AllowedDecimal"/> as a numeric limit.
/// </summary>
public enum RuleType
{
    Boolean = 0,
    Integer = 1,
    Decimal = 2
}
