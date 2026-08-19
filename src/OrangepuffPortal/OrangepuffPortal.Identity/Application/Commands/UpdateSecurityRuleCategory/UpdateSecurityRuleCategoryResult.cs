namespace OrangepuffPortal.Identity.Application.Commands.UpdateSecurityRuleCategory
{
    /// <summary>
    /// Outcome of <see cref="UpdateSecurityRuleCategoryCommand"/>.
    /// </summary>
    public record UpdateSecurityRuleCategoryResult(bool Success, string? RejectionReason)
    {
        public static UpdateSecurityRuleCategoryResult Updated() => new(true, null);

        public static UpdateSecurityRuleCategoryResult Rejected(string reason) => new(false, reason);
    }
}
