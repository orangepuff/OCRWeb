namespace OrangepuffPortal.Identity.Application.Commands.AddSecurityRuleCategory
{
    /// <summary>
    /// Outcome of <see cref="AddSecurityRuleCategoryCommand"/>.
    /// </summary>
    public record AddSecurityRuleCategoryResult(bool Success, int? CategoryId, string? RejectionReason)
    {
        public static AddSecurityRuleCategoryResult Created(int categoryId) => new(true, categoryId, null);

        public static AddSecurityRuleCategoryResult Rejected(string reason) => new(false, null, reason);
    }
}
