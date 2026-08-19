using MediatR;

namespace OrangepuffPortal.Identity.Application.Commands.DeleteSecurityRuleCategory
{
    public record DeleteSecurityRuleCategoryCommand(int CategoryId) : IRequest<DeleteSecurityRuleCategoryResult>;
}
