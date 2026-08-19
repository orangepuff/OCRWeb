namespace OrangepuffPortal.Identity.Application.Commands.UpdateUserAvatar
{
    /// <summary>
    /// Outcome of <see cref="UpdateUserAvatarCommand"/>.
    /// </summary>
    public record UpdateUserAvatarResult(bool Success, string? RejectionReason)
    {
        public static UpdateUserAvatarResult Updated() => new(true, null);

        public static UpdateUserAvatarResult Rejected(string reason) => new(false, reason);
    }
}
