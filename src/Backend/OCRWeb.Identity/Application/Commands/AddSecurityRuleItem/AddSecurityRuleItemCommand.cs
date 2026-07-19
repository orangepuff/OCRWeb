using MediatR;
using OCRWeb.Identity.Domain.Enums;

namespace OCRWeb.Identity.Application.Commands.AddSecurityRuleItem
{
    public record AddSecurityRuleItemCommand(
        int CategoryId, string Code, string Description, RuleType RuleType, string? TextCode, int? SortOrder) : IRequest<AddSecurityRuleItemResult>;
}
