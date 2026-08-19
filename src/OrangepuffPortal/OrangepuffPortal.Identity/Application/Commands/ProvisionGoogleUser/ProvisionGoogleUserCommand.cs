using MediatR;

namespace OrangepuffPortal.Identity.Application.Commands.ProvisionGoogleUser
{
    /// <summary>
    /// Resolves or creates the local user for a successful Google sign-in.
    /// </summary>
    public record ProvisionGoogleUserCommand(string ProviderKey, string Email, bool EmailVerified, string? DisplayName) : IRequest<ProvisionGoogleUserResult>;
}
