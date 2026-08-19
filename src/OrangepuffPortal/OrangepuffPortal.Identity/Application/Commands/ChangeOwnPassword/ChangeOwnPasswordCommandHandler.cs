using Diagnostics.Abstractions.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OrangepuffPortal.Identity.Domain.Entity;
using OrangepuffPortal.Identity.Domain.Repositories;

namespace OrangepuffPortal.Identity.Application.Commands.ChangeOwnPassword
{
    public class ChangeOwnPasswordCommandHandler(
        IUserRepository repository
        , IPasswordHasher<User> passwordHasher
        , ITransactionLogger transactionLogger
        , ILogger<ChangeOwnPasswordCommandHandler> logger) : IRequestHandler<ChangeOwnPasswordCommand, ChangeOwnPasswordResult>
    {
        private const string LogPrefix = nameof(ChangeOwnPasswordCommandHandler) + "." + nameof(Handle);

        public async Task<ChangeOwnPasswordResult> Handle(ChangeOwnPasswordCommand request, CancellationToken cancellationToken)
        {
            using var transaction = transactionLogger.BeginTransaction("ChangeOwnPassword", $"Change own password for user {request.UserId}");
            transaction.SetUser(request.UserId.ToString());

            var user = await repository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                logger.LogWarning("{LogPrefix}: user {UserId} not found", LogPrefix, request.UserId);
                transaction.SetCustomAttribute("outcome", "user_not_found");
                return ChangeOwnPasswordResult.Rejected("user_not_found");
            }

            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                logger.LogWarning("{LogPrefix}: rejected, user {UserId} has no password set (Google-only account)", LogPrefix, request.UserId);
                transaction.SetCustomAttribute("outcome", "no_password_set");
                return ChangeOwnPasswordResult.Rejected("no_password_set");
            }

            var verification = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
            if (verification == PasswordVerificationResult.Failed)
            {
                logger.LogWarning("{LogPrefix}: rejected, wrong current password for user {UserId}", LogPrefix, request.UserId);
                transaction.SetCustomAttribute("outcome", "invalid_current_password");
                return ChangeOwnPasswordResult.Rejected("invalid_current_password");
            }

            if (string.IsNullOrWhiteSpace(request.NewPassword))
            {
                logger.LogWarning("{LogPrefix}: rejected, empty new password for user {UserId}", LogPrefix, request.UserId);
                transaction.SetCustomAttribute("outcome", "password_required");
                return ChangeOwnPasswordResult.Rejected("password_required");
            }

            user.SetPasswordHash(passwordHasher.HashPassword(user, request.NewPassword), DateTime.UtcNow);
            await repository.SaveChangesAsync(cancellationToken);

            logger.LogInformation("{LogPrefix}: password changed for user {UserId}", LogPrefix, user.Id);
            return ChangeOwnPasswordResult.Updated();
        }
    }
}
