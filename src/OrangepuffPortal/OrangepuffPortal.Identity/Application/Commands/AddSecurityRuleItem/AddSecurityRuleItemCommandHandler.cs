using Diagnostics.Abstractions.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using OrangepuffPortal.Identity.Domain.Entity;
using OrangepuffPortal.Identity.Domain.Repositories;
using OrangepuffPortal.Shared.Auditing;

namespace OrangepuffPortal.Identity.Application.Commands.AddSecurityRuleItem
{
    public class AddSecurityRuleItemCommandHandler(
        ISecurityRuleItemRepository itemRepository
        , ISecurityRuleCategoryRepository categoryRepository
        , ICurrentUser currentUser
        , ITransactionLogger transactionLogger
        , ILogger<AddSecurityRuleItemCommandHandler> logger) : IRequestHandler<AddSecurityRuleItemCommand, AddSecurityRuleItemResult>
    {
        private const string LogPrefix = nameof(AddSecurityRuleItemCommandHandler) + "." + nameof(Handle);

        public async Task<AddSecurityRuleItemResult> Handle(AddSecurityRuleItemCommand request, CancellationToken cancellationToken)
        {
            using var transaction = transactionLogger.BeginTransaction("AddSecurityRuleItem", $"Add rule item {request.Code}");

            if (await categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken) is null)
            {
                logger.LogWarning("{LogPrefix}: rejected, category {CategoryId} not found", LogPrefix, request.CategoryId);
                transaction.SetCustomAttribute("outcome", "category_not_found");
                return AddSecurityRuleItemResult.Rejected("category_not_found");
            }

            if (await itemRepository.GetByCodeAsync(request.Code, cancellationToken) is not null)
            {
                logger.LogWarning("{LogPrefix}: rejected, code {Code} already taken", LogPrefix, request.Code);
                transaction.SetCustomAttribute("outcome", "code_taken");
                return AddSecurityRuleItemResult.Rejected("code_taken");
            }

            var item = new SecurityRuleItem(
                request.CategoryId, request.Code, request.Description, request.RuleType, request.TextCode, request.SortOrder,
                currentUser.UserId, DateTime.UtcNow);

            await itemRepository.AddAsync(item, cancellationToken);
            await itemRepository.SaveChangesAsync(cancellationToken);

            logger.LogInformation("{LogPrefix}: created rule item {RuleItemId}", LogPrefix, item.Id);
            return AddSecurityRuleItemResult.Created(item.Id);
        }
    }
}
