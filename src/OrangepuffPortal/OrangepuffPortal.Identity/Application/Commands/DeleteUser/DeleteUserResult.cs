namespace OrangepuffPortal.Identity.Application.Commands.DeleteUser
{
    /// <summary>
    /// Outcome of <see cref="DeleteUserCommand"/>.
    /// </summary>
    public record DeleteUserResult(bool Success, string? RejectionReason)
    {
        public static DeleteUserResult Deleted() => new(true, null);

        public static DeleteUserResult Rejected(string reason) => new(false, reason);
    }
}
