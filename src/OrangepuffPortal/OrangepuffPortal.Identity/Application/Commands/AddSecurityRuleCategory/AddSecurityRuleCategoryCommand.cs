using MediatR;

namespace OrangepuffPortal.Identity.Application.Commands.AddSecurityRuleCategory
{
    public record AddSecurityRuleCategoryCommand(string CategoryDesc, string? TextCode) : IRequest<AddSecurityRuleCategoryResult>;
}
