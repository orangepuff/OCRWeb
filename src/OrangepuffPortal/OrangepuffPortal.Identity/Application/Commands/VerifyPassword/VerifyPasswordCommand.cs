using MediatR;

namespace OrangepuffPortal.Identity.Application.Commands.VerifyPassword
{
    /// <summary>
    /// Local username/email + password sign-in attempt (as opposed to Google OAuth).
    /// </summary>
    public record VerifyPasswordCommand(string UsernameOrEmail, string Password) : IRequest<VerifyPasswordResult>;
}
