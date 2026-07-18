namespace OCRWeb.Identity.Application.Commands.ProvisionGoogleUser
{
    /// <summary>
    /// Outcome of <see cref="ProvisionGoogleUserCommand"/>.
    /// </summary>
    public record ProvisionGoogleUserResult(bool Success, int? UserId, string? RejectionReason)
    {
        public static ProvisionGoogleUserResult Allowed(int userId) => new(true, userId, null);

        public static ProvisionGoogleUserResult Rejected(string reason) => new(false, null, reason);
    }
}
