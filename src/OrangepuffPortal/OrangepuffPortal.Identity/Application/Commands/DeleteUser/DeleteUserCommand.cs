using MediatR;

namespace OrangepuffPortal.Identity.Application.Commands.DeleteUser
{
    /// <summary>
    /// Hard-deletes a user. Rejected if the user is a template that other users still inherit from.
    /// </summary>
    public record DeleteUserCommand(int UserId) : IRequest<DeleteUserResult>;
}
