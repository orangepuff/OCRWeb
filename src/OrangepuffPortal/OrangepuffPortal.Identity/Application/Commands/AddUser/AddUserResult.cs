namespace OrangepuffPortal.Identity.Application.Commands.AddUser
{
    /// <summary>
    /// Outcome of <see cref="AddUserCommand"/>.
    /// </summary>
    public record AddUserResult(bool Success, int? UserId, string? RejectionReason)
    {
        public static AddUserResult Created(int userId) => new(true, userId, null);

        public static AddUserResult Rejected(string reason) => new(false, null, reason);
    }
}
