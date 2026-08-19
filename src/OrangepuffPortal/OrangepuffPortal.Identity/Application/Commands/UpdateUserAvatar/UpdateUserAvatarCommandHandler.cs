using Diagnostics.Abstractions.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using OrangepuffPortal.Identity.Domain.Repositories;

namespace OrangepuffPortal.Identity.Application.Commands.UpdateUserAvatar
{
    public class UpdateUserAvatarCommandHandler(
        IUserRepository repository
        , ITransactionLogger transactionLogger
        , ILogger<UpdateUserAvatarCommandHandler> logger) : IRequestHandler<UpdateUserAvatarCommand, UpdateUserAvatarResult>
    {
        private const int MaxAvatarBytes = 2 * 1024 * 1024;
        private const string LogPrefix = nameof(UpdateUserAvatarCommandHandler) + "." + nameof(Handle);

        public async Task<UpdateUserAvatarResult> Handle(UpdateUserAvatarCommand request, CancellationToken cancellationToken)
        {
            using var transaction = transactionLogger.BeginTransaction("UpdateUserAvatar", $"Update avatar for user {request.UserId}");
            transaction.SetUser(request.UserId.ToString());

            if (await repository.GetByIdAsync(request.UserId, cancellationToken) is null)
            {
                logger.LogWarning("{LogPrefix}: user {UserId} not found", LogPrefix, request.UserId);
                transaction.SetCustomAttribute("outcome", "user_not_found");
                return UpdateUserAvatarResult.Rejected("user_not_found");
            }

            if (request.Image is not null && request.Image.Length > MaxAvatarBytes)
            {
                logger.LogWarning("{LogPrefix}: rejected, avatar for user {UserId} is {Size} bytes, over the {Max} byte cap",
                    LogPrefix, request.UserId, request.Image.Length, MaxAvatarBytes);
                transaction.SetCustomAttribute("outcome", "avatar_too_large");
                return UpdateUserAvatarResult.Rejected("avatar_too_large");
            }

            await repository.SetAvatarAsync(request.UserId, request.Image, request.ContentType, DateTime.UtcNow, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);

            logger.LogInformation("{LogPrefix}: updated avatar for user {UserId}", LogPrefix, request.UserId);
            return UpdateUserAvatarResult.Updated();
        }
    }
}
