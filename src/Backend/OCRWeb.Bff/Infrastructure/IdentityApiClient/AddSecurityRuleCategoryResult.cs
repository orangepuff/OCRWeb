namespace OCRWeb.Bff.Infrastructure.IdentityApiClient
{
    public record AddSecurityRuleCategoryResult(bool Success, int? CategoryId, string? RejectionReason);
}
