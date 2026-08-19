using MediatR;

namespace OrangepuffPortal.Identity.Application.Commands.UpdateUserAvatar
{
    /// <summary>
    /// Self-service: sets or clears (pass a null <paramref name="Image"/>) a user's profile picture.
    /// </summary>
    public record UpdateUserAvatarCommand(int UserId, byte[]? Image, string? ContentType) : IRequest<UpdateUserAvatarResult>;
}
