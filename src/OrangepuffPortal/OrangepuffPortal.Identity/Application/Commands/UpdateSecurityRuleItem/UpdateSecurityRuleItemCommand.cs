using MediatR;
using OrangepuffPortal.Identity.Domain.Enums;

namespace OrangepuffPortal.Identity.Application.Commands.UpdateSecurityRuleItem
{
    public record UpdateSecurityRuleItemCommand(
        int RuleItemId, int CategoryId, string Description, RuleType RuleType, string? TextCode, int? SortOrder, bool Hidden) : IRequest<UpdateSecurityRuleItemResult>;
}
