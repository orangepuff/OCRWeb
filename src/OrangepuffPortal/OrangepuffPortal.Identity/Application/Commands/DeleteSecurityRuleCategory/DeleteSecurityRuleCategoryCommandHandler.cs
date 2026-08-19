using Diagnostics.Abstractions.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using OrangepuffPortal.Identity.Domain.Repositories;

namespace OrangepuffPortal.Identity.Application.Commands.DeleteSecurityRuleCategory
{
    public class DeleteSecurityRuleCategoryCommandHandler(
        ISecurityRuleCategoryRepository repository
        , ITransactionLogger transactionLogger
        , ILogger<DeleteSecurityRuleCategoryCommandHandler> logger) : IRequestHandler<DeleteSecurityRuleCategoryCommand, DeleteSecurityRuleCategoryResult>
    {
        private const string LogPrefix = nameof(DeleteSecurityRuleCategoryCommandHandler) + "." + nameof(Handle);

        public async Task<DeleteSecurityRuleCategoryResult> Handle(DeleteSecurityRuleCategoryCommand request, CancellationToken cancellationToken)
        {
            using var transaction = transactionLogger.BeginTransaction("DeleteSecurityRuleCategory", $"Delete category {request.CategoryId}");

            var category = await repository.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category is null)
            {
                logger.LogWarning("{LogPrefix}: category {CategoryId} not found", LogPrefix, request.CategoryId);
                transaction.SetCustomAttribute("outcome", "category_not_found");
                return DeleteSecurityRuleCategoryResult.Rejected("category_not_found");
            }

            if (await repository.HasItemsAsync(category.Id, cancellationToken))
            {
                logger.LogWarning("{LogPrefix}: rejected, category {CategoryId} still has rule items", LogPrefix, category.Id);
                transaction.SetCustomAttribute("outcome", "has_items");
                return DeleteSecurityRuleCategoryResult.Rejected("has_items");
            }

            await repository.DeleteAsync(category, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);

            logger.LogInformation("{LogPrefix}: deleted category {CategoryId}", LogPrefix, category.Id);
            return DeleteSecurityRuleCategoryResult.Deleted();
        }
    }
}
