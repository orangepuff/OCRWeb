using Diagnostics.Abstractions.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using OCRWeb.Identity.Domain.Repositories;

namespace OCRWeb.Identity.Application.Commands.UpdateUser
{
    public class UpdateUserCommandHandler(
        IUserRepository userRepository
        , ISecurityUserRuleItemRepository userRuleItemRepository
        , ITransactionLogger transactionLogger
        , ILogger<UpdateUserCommandHandler> logger) : IRequestHandler<UpdateUserCommand, UpdateUserResult>
    {
        private const string LogPrefix = nameof(UpdateUserCommandHandler) + "." + nameof(Handle);

        public async Task<UpdateUserResult> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            using var transaction = transactionLogger.BeginTransaction("UpdateUser", $"Update user {request.UserId}");
            transaction.SetUser(request.UserId.ToString());

            var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                logger.LogWarning("{LogPrefix}: user {UserId} not found", LogPrefix, request.UserId);
                transaction.SetCustomAttribute("outcome", "user_not_found");
                return UpdateUserResult.Rejected("user_not_found");
            }

            if (request.IsTemplateUser && request.ParentId is not null)
            {
                logger.LogWarning("{LogPrefix}: rejected, a template user cannot itself have a parent", LogPrefix);
                transaction.SetCustomAttribute("outcome", "template_cannot_have_parent");
                return UpdateUserResult.Rejected("template_cannot_have_parent");
            }

            if (request.ParentId is int parentId)
            {
                if (parentId == user.Id)
                {
                    logger.LogWarning("{LogPrefix}: rejected, user {UserId} cannot be its own template", LogPrefix, user.Id);
                    transaction.SetCustomAttribute("outcome", "not_a_template_user");
                    return UpdateUserResult.Rejected("not_a_template_user");
                }

                var templateUser = await userRepository.GetByIdAsync(parentId, cancellationToken);
                if (templateUser is null)
                {
                    logger.LogWarning("{LogPrefix}: rejected, template user {TemplateUserId} not found", LogPrefix, parentId);
                    transaction.SetCustomAttribute("outcome", "template_user_not_found");
                    return UpdateUserResult.Rejected("template_user_not_found");
                }

                if (!templateUser.IsTemplateUser || templateUser.ParentId is not null)
                {
                    logger.LogWarning("{LogPrefix}: rejected, user {TemplateUserId} is not a usable template", LogPrefix, parentId);
                    transaction.SetCustomAttribute("outcome", "not_a_template_user");
                    return UpdateUserResult.Rejected("not_a_template_user");
                }

                if (await userRepository.HasChildUsersAsync(user.Id, cancellationToken))
                {
                    logger.LogWarning("{LogPrefix}: rejected, user {UserId} still has its own dependent users", LogPrefix, user.Id);
                    transaction.SetCustomAttribute("outcome", "has_dependent_users");
                    return UpdateUserResult.Rejected("has_dependent_users");
                }

                if (await userRuleItemRepository.HasAnyForUserAsync(user.Id, cancellationToken))
                {
                    logger.LogWarning("{LogPrefix}: rejected, user {UserId} still has its own permission assignments", LogPrefix, user.Id);
                    transaction.SetCustomAttribute("outcome", "has_own_permissions");
                    return UpdateUserResult.Rejected("has_own_permissions");
                }
            }

            if (user.IsTemplateUser && !request.IsTemplateUser && await userRepository.HasChildUsersAsync(user.Id, cancellationToken))
            {
                logger.LogWarning("{LogPrefix}: rejected, user {UserId} is still used as a template by other users", LogPrefix, user.Id);
                transaction.SetCustomAttribute("outcome", "still_used_as_template");
                return UpdateUserResult.Rejected("still_used_as_template");
            }

            var now = DateTime.UtcNow;
            user.UpdateProfile(request.Email, request.DisplayName, now);
            user.SetParent(request.ParentId, now);

            if (request.IsTemplateUser && !user.IsTemplateUser)
            {
                user.MarkAsTemplateUser(now);
            }
            else if (!request.IsTemplateUser && user.IsTemplateUser)
            {
                user.UnmarkAsTemplateUser(now);
            }

            await userRepository.SaveChangesAsync(cancellationToken);

            logger.LogInformation("{LogPrefix}: updated user {UserId}", LogPrefix, user.Id);
            return UpdateUserResult.Updated();
        }
    }
}
