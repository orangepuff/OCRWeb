using MediatR;

namespace OCRWeb.Identity.Application.Commands.AddUser
{
    /// <summary>
    /// Admin-created user. If <paramref name="TemplateUserId"/> is set, the new user inherits
    /// permissions from that template user instead of getting its own SecurityUserRuleItems rows.
    /// </summary>
    public record AddUserCommand(string Username, string? Email, string? DisplayName, int? TemplateUserId) : IRequest<AddUserResult>;
}
