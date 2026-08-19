namespace OrangepuffPortal.Identity.Application.Commands.UpdateDisplayName
{
    /// <summary>
    /// Outcome of <see cref="UpdateDisplayNameCommand"/>.
    /// </summary>
    public record UpdateDisplayNameResult(bool Success, string? RejectionReason)
    {
        public static UpdateDisplayNameResult Updated() => new(true, null);

        public static UpdateDisplayNameResult Rejected(string reason) => new(false, reason);
    }
}
