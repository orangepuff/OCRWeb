using MediatR;

namespace OrangepuffPortal.Identity.Application.Commands.ChangeOwnPassword
{
    /// <summary>
    /// Self-service password change — unlike <c>SetUserPasswordCommand</c> (admin-only reset, no
    /// current-password check), this requires the caller to prove they know their current password.
    /// </summary>
    public record ChangeOwnPasswordCommand(int UserId, string CurrentPassword, string NewPassword) : IRequest<ChangeOwnPasswordResult>;
}
