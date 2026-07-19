using MediatR;

namespace OCRWeb.Identity.Application.Commands.DeleteSecurityRuleCategory
{
    public record DeleteSecurityRuleCategoryCommand(int CategoryId) : IRequest<DeleteSecurityRuleCategoryResult>;
}
