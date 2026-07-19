using MediatR;

namespace OCRWeb.Identity.Application.Commands.DeleteSecurityRuleItem
{
    public record DeleteSecurityRuleItemCommand(int RuleItemId) : IRequest<DeleteSecurityRuleItemResult>;
}
