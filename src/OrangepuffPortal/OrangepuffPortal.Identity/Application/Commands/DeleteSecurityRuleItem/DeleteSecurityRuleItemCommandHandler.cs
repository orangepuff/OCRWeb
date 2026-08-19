using Diagnostics.Abstractions.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using OrangepuffPortal.Identity.Domain.Repositories;

namespace OrangepuffPortal.Identity.Application.Commands.DeleteSecurityRuleItem
{
    public class DeleteSecurityRuleItemCommandHandler(
        ISecurityRuleItemRepository itemRepository
        , ISecurityUserRuleItemRepository userRuleItemRepository
        , ITransactionLogger transactionLogger
        , ILogger<DeleteSecurityRuleItemCommandHandler> logger) : IRequestHandler<DeleteSecurityRuleItemCommand, DeleteSecurityRuleItemResult>
    {
        private const string LogPrefix = nameof(DeleteSecurityRuleItemCommandHandler) + "." + nameof(Handle);

        public async Task<DeleteSecurityRuleItemResult> Handle(DeleteSecurityRuleItemCommand request, CancellationToken cancellationToken)
        {
            using var transaction = transactionLogger.BeginTransaction("DeleteSecurityRuleItem", $"Delete rule item {request.RuleItemId}");

            var item = await itemRepository.GetByIdAsync(request.RuleItemId, cancellationToken);
            if (item is null)
            {
                logger.LogWarning("{LogPrefix}: rule item {RuleItemId} not found", LogPrefix, request.RuleItemId);
                transaction.SetCustomAttribute("outcome", "rule_item_not_found");
                return DeleteSecurityRuleItemResult.Rejected("rule_item_not_found");
            }

            if (await userRuleItemRepository.HasAnyForRuleItemAsync(item.Id, cancellationToken))
            {
                logger.LogWarning("{LogPrefix}: rejected, rule item {RuleItemId} still has user assignments", LogPrefix, item.Id);
                transaction.SetCustomAttribute("outcome", "has_assignments");
                return DeleteSecurityRuleItemResult.Rejected("has_assignments");
            }

            await itemRepository.DeleteAsync(item, cancellationToken);
            await itemRepository.SaveChangesAsync(cancellationToken);

            logger.LogInformation("{LogPrefix}: deleted rule item {RuleItemId}", LogPrefix, item.Id);
            return DeleteSecurityRuleItemResult.Deleted();
        }
    }
}
