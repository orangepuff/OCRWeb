namespace OrangepuffPortal.Identity.Application.Commands.DeleteSecurityRuleCategory
{
    /// <summary>
    /// Outcome of <see cref="DeleteSecurityRuleCategoryCommand"/>.
    /// </summary>
    public record DeleteSecurityRuleCategoryResult(bool Success, string? RejectionReason)
    {
        public static DeleteSecurityRuleCategoryResult Deleted() => new(true, null);

        public static DeleteSecurityRuleCategoryResult Rejected(string reason) => new(false, reason);
    }
}
