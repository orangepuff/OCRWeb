using MediatR;

namespace OrangepuffPortal.Identity.Application.Commands.UpdateDisplayName
{
    /// <summary>
    /// Self-service display name change — unlike <c>UpdateUserCommand</c> (admin-only, also touches
    /// active/template/parent fields), this only ever changes the caller's own display name.
    /// </summary>
    public record UpdateDisplayNameCommand(int UserId, string DisplayName) : IRequest<UpdateDisplayNameResult>;
}
