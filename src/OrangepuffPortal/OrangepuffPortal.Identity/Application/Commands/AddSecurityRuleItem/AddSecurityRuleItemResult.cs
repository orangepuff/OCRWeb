namespace OrangepuffPortal.Identity.Application.Commands.AddSecurityRuleItem
{
    /// <summary>
    /// Outcome of <see cref="AddSecurityRuleItemCommand"/>.
    /// </summary>
    public record AddSecurityRuleItemResult(bool Success, int? RuleItemId, string? RejectionReason)
    {
        public static AddSecurityRuleItemResult Created(int ruleItemId) => new(true, ruleItemId, null);

        public static AddSecurityRuleItemResult Rejected(string reason) => new(false, null, reason);
    }
}
