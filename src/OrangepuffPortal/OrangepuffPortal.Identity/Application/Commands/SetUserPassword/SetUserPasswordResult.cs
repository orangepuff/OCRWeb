namespace OrangepuffPortal.Identity.Application.Commands.SetUserPassword
{
    /// <summary>
    /// Outcome of <see cref="SetUserPasswordCommand"/>.
    /// </summary>
    public record SetUserPasswordResult(bool Success, string? RejectionReason)
    {
        public static SetUserPasswordResult Updated() => new(true, null);

        public static SetUserPasswordResult Rejected(string reason) => new(false, reason);
    }
}
