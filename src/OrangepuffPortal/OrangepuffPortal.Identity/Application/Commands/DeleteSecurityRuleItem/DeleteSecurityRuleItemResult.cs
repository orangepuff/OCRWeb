namespace OrangepuffPortal.Identity.Application.Commands.DeleteSecurityRuleItem
{
    /// <summary>
    /// Outcome of <see cref="DeleteSecurityRuleItemCommand"/>.
    /// </summary>
    public record DeleteSecurityRuleItemResult(bool Success, string? RejectionReason)
    {
        public static DeleteSecurityRuleItemResult Deleted() => new(true, null);

        public static DeleteSecurityRuleItemResult Rejected(string reason) => new(false, reason);
    }
}
