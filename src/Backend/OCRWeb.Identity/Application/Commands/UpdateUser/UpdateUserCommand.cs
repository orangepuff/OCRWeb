using MediatR;

namespace OCRWeb.Identity.Application.Commands.UpdateUser
{
    /// <summary>
    /// Updates profile fields plus template-user assignment. Pass <paramref name="ParentId"/> as null
    /// to keep the user independent (or to clear an existing template link).
    /// </summary>
    public record UpdateUserCommand(int UserId, string? Email, string? DisplayName, bool IsTemplateUser, int? ParentId) : IRequest<UpdateUserResult>;
}
