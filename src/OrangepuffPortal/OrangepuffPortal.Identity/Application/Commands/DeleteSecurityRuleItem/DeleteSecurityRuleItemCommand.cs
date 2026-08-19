using MediatR;

namespace OrangepuffPortal.Identity.Application.Commands.DeleteSecurityRuleItem
{
    public record DeleteSecurityRuleItemCommand(int RuleItemId) : IRequest<DeleteSecurityRuleItemResult>;
}
