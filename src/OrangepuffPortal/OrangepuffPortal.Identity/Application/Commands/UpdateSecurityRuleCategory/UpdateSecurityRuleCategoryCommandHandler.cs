using Diagnostics.Abstractions.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using OrangepuffPortal.Identity.Domain.Repositories;
using OrangepuffPortal.Shared.Auditing;

namespace OrangepuffPortal.Identity.Application.Commands.UpdateSecurityRuleCategory
{
    public class UpdateSecurityRuleCategoryCommandHandler(
        ISecurityRuleCategoryRepository repository
        , ICurrentUser currentUser
        , ITransactionLogger transactionLogger
        , ILogger<UpdateSecurityRuleCategoryCommandHandler> logger) : IRequestHandler<UpdateSecurityRuleCategoryCommand, UpdateSecurityRuleCategoryResult>
    {
        private const string LogPrefix = nameof(UpdateSecurityRuleCategoryCommandHandler) + "." + nameof(Handle);

        public async Task<UpdateSecurityRuleCategoryResult> Handle(UpdateSecurityRuleCategoryCommand request, CancellationToken cancellationToken)
        {
            using var transaction = transactionLogger.BeginTransaction("UpdateSecurityRuleCategory", $"Update category {request.CategoryId}");

            var category = await repository.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category is null)
            {
                logger.LogWarning("{LogPrefix}: category {CategoryId} not found", LogPrefix, request.CategoryId);
                transaction.SetCustomAttribute("outcome", "category_not_found");
                return UpdateSecurityRuleCategoryResult.Rejected("category_not_found");
            }

            var existingByDesc = await repository.GetByDescAsync(request.CategoryDesc, cancellationToken);
            if (existingByDesc is not null && existingByDesc.Id != request.CategoryId)
            {
                logger.LogWarning("{LogPrefix}: rejected, category desc {CategoryDesc} already taken", LogPrefix, request.CategoryDesc);
                transaction.SetCustomAttribute("outcome", "category_desc_taken");
                return UpdateSecurityRuleCategoryResult.Rejected("category_desc_taken");
            }

            var now = DateTime.UtcNow;
            category.UpdateDetails(request.CategoryDesc, request.TextCode, currentUser.UserId, now);
            category.SetHidden(request.Hidden, currentUser.UserId, now);
            await repository.SaveChangesAsync(cancellationToken);

            logger.LogInformation("{LogPrefix}: updated category {CategoryId}", LogPrefix, category.Id);
            return UpdateSecurityRuleCategoryResult.Updated();
        }
    }
}
