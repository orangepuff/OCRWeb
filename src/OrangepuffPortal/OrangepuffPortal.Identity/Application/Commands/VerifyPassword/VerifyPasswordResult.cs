namespace OrangepuffPortal.Identity.Application.Commands.VerifyPassword
{
    /// <summary>
    /// Outcome of <see cref="VerifyPasswordCommand"/>. RejectionReason is deliberately generic
    /// ("invalid_credentials") for both "no such account" and "wrong password" to avoid username
    /// enumeration — "account_inactive" is the one reason exposed distinctly, matching how other
    /// Identity results already surface specific rejection reasons.
    /// </summary>
    public record VerifyPasswordResult(bool Success, int? UserId, string? Email, string? DisplayName, string? CultureCode, string? RejectionReason)
    {
        public static VerifyPasswordResult Allowed(int userId, string? email, string? displayName, string cultureCode) => new(true, userId, email, displayName, cultureCode, null);

        public static VerifyPasswordResult Rejected(string reason) => new(false, null, null, null, null, reason);
    }
}
