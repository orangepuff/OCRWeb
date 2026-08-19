namespace OrangepuffPortal.Identity.Application.Commands.ProvisionGoogleUser
{
    /// <summary>
    /// Outcome of <see cref="ProvisionGoogleUserCommand"/>.
    /// </summary>
    public record ProvisionGoogleUserResult(bool Success, int? UserId, string? CultureCode, string? RejectionReason)
    {
        public static ProvisionGoogleUserResult Allowed(int userId, string cultureCode) => new(true, userId, cultureCode, null);

        public static ProvisionGoogleUserResult Rejected(string reason) => new(false, null, null, reason);
    }
}
