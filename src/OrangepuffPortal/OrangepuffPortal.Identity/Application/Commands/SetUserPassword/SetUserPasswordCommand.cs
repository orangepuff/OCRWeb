using MediatR;

namespace OrangepuffPortal.Identity.Application.Commands.SetUserPassword
{
    /// <summary>
    /// Sets (or resets) a user's local password. Used both when an admin sets/resets another
    /// user's password and when a new user is created with one (see AddUserCommand).
    /// </summary>
    public record SetUserPasswordCommand(int UserId, string NewPassword) : IRequest<SetUserPasswordResult>;
}
