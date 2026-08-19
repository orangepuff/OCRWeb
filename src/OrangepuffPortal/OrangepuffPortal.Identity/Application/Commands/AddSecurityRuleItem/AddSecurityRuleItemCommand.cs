using MediatR;
using OrangepuffPortal.Identity.Domain.Enums;

namespace OrangepuffPortal.Identity.Application.Commands.AddSecurityRuleItem
{
    public record AddSecurityRuleItemCommand(
        int CategoryId, string Code, string Description, RuleType RuleType, string? TextCode, int? SortOrder) : IRequest<AddSecurityRuleItemResult>;
}
