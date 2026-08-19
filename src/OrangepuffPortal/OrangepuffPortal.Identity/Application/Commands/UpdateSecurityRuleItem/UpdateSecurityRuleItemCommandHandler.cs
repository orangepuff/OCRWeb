using Diagnostics.Abstractions.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using OrangepuffPortal.Identity.Domain.Repositories;
using OrangepuffPortal.Shared.Auditing;

namespace OrangepuffPortal.Identity.Application.Commands.UpdateSecurityRuleItem
{
    public class UpdateSecurityRuleItemCommandHandler(
        ISecurityRuleItemRepository itemRepository
        , ISecurityRuleCategoryRepository categoryRepository
        , ICurrentUser currentUser
        , ITransactionLogger transactionLogger
        , ILogger<UpdateSecurityRuleItemCommandHandler> logger) : IRequestHandler<UpdateSecurityRuleItemCommand, UpdateSecurityRuleItemResult>
    {
        private const string LogPrefix = nameof(UpdateSecurityRuleItemCommandHandler) + "." + nameof(Handle);

        public async Task<UpdateSecurityRuleItemResult> Handle(UpdateSecurityRuleItemCommand request, CancellationToken cancellationToken)
        {
            using var transaction = transactionLogger.BeginTransaction("UpdateSecurityRuleItem", $"Update rule item {request.RuleItemId}");

            var item = await itemRepository.GetByIdAsync(request.RuleItemId, cancellationToken);
            if (item is null)
            {
                logger.LogWarning("{LogPrefix}: rule item {RuleItemId} not found", LogPrefix, request.RuleItemId);
                transaction.SetCustomAttribute("outcome", "rule_item_not_found");
                return UpdateSecurityRuleItemResult.Rejected("rule_item_not_found");
            }

            if (await categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken) is null)
            {
                logger.LogWarning("{LogPrefix}: rejected, category {CategoryId} not found", LogPrefix, request.CategoryId);
                transaction.SetCustomAttribute("outcome", "category_not_found");
                return UpdateSecurityRuleItemResult.Rejected("category_not_found");
            }

            var now = DateTime.UtcNow;
            item.UpdateDetails(request.CategoryId, request.Description, request.RuleType, request.TextCode, request.SortOrder, currentUser.UserId, now);
            item.SetHidden(request.Hidden, currentUser.UserId, now);
            await itemRepository.SaveChangesAsync(cancellationToken);

            logger.LogInformation("{LogPrefix}: updated rule item {RuleItemId}", LogPrefix, item.Id);
            return UpdateSecurityRuleItemResult.Updated();
        }
    }
}
