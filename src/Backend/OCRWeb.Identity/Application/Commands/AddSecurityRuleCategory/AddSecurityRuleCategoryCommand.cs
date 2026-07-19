using MediatR;

namespace OCRWeb.Identity.Application.Commands.AddSecurityRuleCategory
{
    public record AddSecurityRuleCategoryCommand(string CategoryDesc, string? TextCode) : IRequest<AddSecurityRuleCategoryResult>;
}
