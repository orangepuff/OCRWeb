using Diagnostics.Abstractions.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using OrangepuffPortal.Identity.Domain.Repositories;

namespace OrangepuffPortal.Identity.Application.Commands.DeleteUser
{
    public class DeleteUserCommandHandler(
        IUserRepository repository
        , ITransactionLogger transactionLogger
        , ILogger<DeleteUserCommandHandler> logger) : IRequestHandler<DeleteUserCommand, DeleteUserResult>
    {
        private const string LogPrefix = nameof(DeleteUserCommandHandler) + "." + nameof(Handle);

        public async Task<DeleteUserResult> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            using var transaction = transactionLogger.BeginTransaction("DeleteUser", $"Delete user {request.UserId}");
            transaction.SetUser(request.UserId.ToString());

            var user = await repository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                logger.LogWarning("{LogPrefix}: user {UserId} not found", LogPrefix, request.UserId);
                transaction.SetCustomAttribute("outcome", "user_not_found");
                return DeleteUserResult.Rejected("user_not_found");
            }

            if (await repository.HasChildUsersAsync(user.Id, cancellationToken))
            {
                logger.LogWarning("{LogPrefix}: rejected delete of user {UserId}, other users still inherit from it as a template", LogPrefix, user.Id);
                transaction.SetCustomAttribute("outcome", "has_dependent_users");
                return DeleteUserResult.Rejected("has_dependent_users");
            }

            await repository.DeleteAsync(user, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);

            logger.LogInformation("{LogPrefix}: deleted user {UserId}", LogPrefix, user.Id);
            return DeleteUserResult.Deleted();
        }
    }
}
