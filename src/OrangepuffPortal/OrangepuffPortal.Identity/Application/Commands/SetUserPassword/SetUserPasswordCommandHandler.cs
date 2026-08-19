using Diagnostics.Abstractions.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OrangepuffPortal.Identity.Domain.Entity;
using OrangepuffPortal.Identity.Domain.Repositories;

namespace OrangepuffPortal.Identity.Application.Commands.SetUserPassword
{
    public class SetUserPasswordCommandHandler(
        IUserRepository repository
        , IPasswordHasher<User> passwordHasher
        , ITransactionLogger transactionLogger
        , ILogger<SetUserPasswordCommandHandler> logger) : IRequestHandler<SetUserPasswordCommand, SetUserPasswordResult>
    {
        private const string LogPrefix = nameof(SetUserPasswordCommandHandler) + "." + nameof(Handle);

        public async Task<SetUserPasswordResult> Handle(SetUserPasswordCommand request, CancellationToken cancellationToken)
        {
            using var transaction = transactionLogger.BeginTransaction("SetUserPassword", $"Set password for user {request.UserId}");
            transaction.SetUser(request.UserId.ToString());

            var user = await repository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                logger.LogWarning("{LogPrefix}: user {UserId} not found", LogPrefix, request.UserId);
                transaction.SetCustomAttribute("outcome", "user_not_found");
                return SetUserPasswordResult.Rejected("user_not_found");
            }

            if (string.IsNullOrWhiteSpace(request.NewPassword))
            {
                logger.LogWarning("{LogPrefix}: rejected, empty password for user {UserId}", LogPrefix, request.UserId);
                transaction.SetCustomAttribute("outcome", "password_required");
                return SetUserPasswordResult.Rejected("password_required");
            }

            user.SetPasswordHash(passwordHasher.HashPassword(user, request.NewPassword), DateTime.UtcNow);
            await repository.SaveChangesAsync(cancellationToken);

            logger.LogInformation("{LogPrefix}: password set for user {UserId}", LogPrefix, user.Id);
            return SetUserPasswordResult.Updated();
        }
    }
}
