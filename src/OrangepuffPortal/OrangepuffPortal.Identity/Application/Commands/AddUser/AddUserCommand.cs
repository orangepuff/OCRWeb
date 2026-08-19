using MediatR;

namespace OrangepuffPortal.Identity.Application.Commands.AddUser
{
    /// <summary>
    /// Admin-created user. If <paramref name="TemplateUserId"/> is set, the new user inherits
    /// permissions from that template user instead of getting its own SecurityUserRuleItems rows.
    /// <paramref name="Password"/> is optional — a user created without one can still sign in via
    /// Google (or have a password set for them later via SetUserPasswordCommand).
    /// </summary>
    /// <param name="ActorUserId">The acting admin's id — attributed on the UserCreatedNotification this publishes.</param>
    public record AddUserCommand(string Username, string? Email, string? DisplayName, int? TemplateUserId, string? Password, int ActorUserId) : IRequest<AddUserResult>;
}
