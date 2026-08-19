using MediatR;

namespace OrangepuffPortal.Identity.Application.Commands.UpdateSecurityRuleCategory
{
    public record UpdateSecurityRuleCategoryCommand(int CategoryId, string CategoryDesc, string? TextCode, bool Hidden) : IRequest<UpdateSecurityRuleCategoryResult>;
}
