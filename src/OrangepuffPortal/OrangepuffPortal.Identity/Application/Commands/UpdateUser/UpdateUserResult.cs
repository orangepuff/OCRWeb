namespace OrangepuffPortal.Identity.Application.Commands.UpdateUser
{
    /// <summary>
    /// Outcome of <see cref="UpdateUserCommand"/>.
    /// </summary>
    public record UpdateUserResult(bool Success, string? RejectionReason)
    {
        public static UpdateUserResult Updated() => new(true, null);

        public static UpdateUserResult Rejected(string reason) => new(false, reason);
    }
}
