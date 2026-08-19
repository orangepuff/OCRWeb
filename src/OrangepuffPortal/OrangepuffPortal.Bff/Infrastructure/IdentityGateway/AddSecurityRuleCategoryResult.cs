namespace OrangepuffPortal.Bff.Infrastructure.IdentityGateway
{
    public record AddSecurityRuleCategoryResult(bool Success, int? CategoryId, string? RejectionReason, string? SuccessMessage);
}
