namespace OrangepuffPortal.Identity.Application.Commands.ChangeOwnPassword
{
    /// <summary>
    /// Outcome of <see cref="ChangeOwnPasswordCommand"/>.
    /// </summary>
    public record ChangeOwnPasswordResult(bool Success, string? RejectionReason)
    {
        public static ChangeOwnPasswordResult Updated() => new(true, null);

        public static ChangeOwnPasswordResult Rejected(string reason) => new(false, reason);
    }
}
