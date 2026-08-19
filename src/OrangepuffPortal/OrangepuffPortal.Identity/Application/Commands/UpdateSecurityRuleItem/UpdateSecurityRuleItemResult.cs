namespace OrangepuffPortal.Identity.Application.Commands.UpdateSecurityRuleItem
{
    /// <summary>
    /// Outcome of <see cref="UpdateSecurityRuleItemCommand"/>.
    /// </summary>
    public record UpdateSecurityRuleItemResult(bool Success, string? RejectionReason)
    {
        public static UpdateSecurityRuleItemResult Updated() => new(true, null);

        public static UpdateSecurityRuleItemResult Rejected(string reason) => new(false, reason);
    }
}
