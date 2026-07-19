using Diagnostics.Abstractions.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using OCRWeb.Identity.Domain.Entity;
using OCRWeb.Identity.Domain.Repositories;

namespace OCRWeb.Identity.Application.Commands.AddUser
{
    public class AddUserCommandHandler(
        IUserRepository repository
        , ITransactionLogger transactionLogger
        , ILogger<AddUserCommandHandler> logger) : IRequestHandler<AddUserCommand, AddUserResult>
    {
        private const string LogPrefix = nameof(AddUserCommandHandler) + "." + nameof(Handle);

        public async Task<AddUserResult> Handle(AddUserCommand request, CancellationToken cancellationToken)
        {
            using var transaction = transactionLogger.BeginTransaction("AddUser", $"Add user {request.Username}");

            if (await repository.GetByUsernameAsync(request.Username, cancellationToken) is not null)
            {
                logger.LogWarning("{LogPrefix}: rejected, username {Username} already taken", LogPrefix, request.Username);
                transaction.SetCustomAttribute("outcome", "username_taken");
                return AddUserResult.Rejected("username_taken");
            }

            if (!string.IsNullOrWhiteSpace(request.Email) && await repository.GetByEmailAsync(request.Email, cancellationToken) is not null)
            {
                logger.LogWarning("{LogPrefix}: rejected, email {Email} already taken", LogPrefix, request.Email);
                transaction.SetCustomAttribute("outcome", "email_taken");
                return AddUserResult.Rejected("email_taken");
            }

            if (request.TemplateUserId is int templateUserId)
            {
                var templateUser = await repository.GetByIdAsync(templateUserId, cancellationToken);
                if (templateUser is null)
                {
                    logger.LogWarning("{LogPrefix}: rejected, template user {TemplateUserId} not found", LogPrefix, templateUserId);
                    transaction.SetCustomAttribute("outcome", "template_user_not_found");
                    return AddUserResult.Rejected("template_user_not_found");
                }

                if (!templateUser.IsTemplateUser || templateUser.ParentId is not null)
                {
                    logger.LogWarning("{LogPrefix}: rejected, user {TemplateUserId} is not a usable template", LogPrefix, templateUserId);
                    transaction.SetCustomAttribute("outcome", "not_a_template_user");
                    return AddUserResult.Rejected("not_a_template_user");
                }
            }

            var now = DateTime.UtcNow;
            var newUser = new User(request.Username, request.Email, request.DisplayName, now);
            if (request.TemplateUserId is int parentId)
            {
                newUser.SetParent(parentId, now);
            }

            await repository.AddAsync(newUser, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);

            transaction.SetUser(newUser.Id.ToString());
            logger.LogInformation("{LogPrefix}: created user {UserId}", LogPrefix, newUser.Id);
            return AddUserResult.Created(newUser.Id);
        }
    }
}
