using Diagnostics.Abstractions.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using OrangepuffPortal.Identity.Domain.Entity;
using OrangepuffPortal.Identity.Domain.Repositories;
using OrangepuffPortal.Shared.Auditing;

namespace OrangepuffPortal.Identity.Application.Commands.AddSecurityRuleCategory
{
    public class AddSecurityRuleCategoryCommandHandler(
        ISecurityRuleCategoryRepository repository
        , ICurrentUser currentUser
        , ITransactionLogger transactionLogger
        , ILogger<AddSecurityRuleCategoryCommandHandler> logger) : IRequestHandler<AddSecurityRuleCategoryCommand, AddSecurityRuleCategoryResult>
    {
        private const string LogPrefix = nameof(AddSecurityRuleCategoryCommandHandler) + "." + nameof(Handle);

        public async Task<AddSecurityRuleCategoryResult> Handle(AddSecurityRuleCategoryCommand request, CancellationToken cancellationToken)
        {
            using var transaction = transactionLogger.BeginTransaction("AddSecurityRuleCategory", $"Add category {request.CategoryDesc}");

            if (await repository.GetByDescAsync(request.CategoryDesc, cancellationToken) is not null)
            {
                logger.LogWarning("{LogPrefix}: rejected, category desc {CategoryDesc} already taken", LogPrefix, request.CategoryDesc);
                transaction.SetCustomAttribute("outcome", "category_desc_taken");
                return AddSecurityRuleCategoryResult.Rejected("category_desc_taken");
            }

            var category = new SecurityRuleCategory(request.CategoryDesc, request.TextCode, currentUser.UserId, DateTime.UtcNow);
            await repository.AddAsync(category, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);

            logger.LogInformation("{LogPrefix}: created category {CategoryId}", LogPrefix, category.Id);
            return AddSecurityRuleCategoryResult.Created(category.Id);
        }
    }
}
