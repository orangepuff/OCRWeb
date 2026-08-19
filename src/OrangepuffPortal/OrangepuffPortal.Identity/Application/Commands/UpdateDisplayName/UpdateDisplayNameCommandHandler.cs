using Diagnostics.Abstractions.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using OrangepuffPortal.Identity.Domain.Repositories;

namespace OrangepuffPortal.Identity.Application.Commands.UpdateDisplayName
{
    public class UpdateDisplayNameCommandHandler(
        IUserRepository repository
        , ITransactionLogger transactionLogger
        , ILogger<UpdateDisplayNameCommandHandler> logger) : IRequestHandler<UpdateDisplayNameCommand, UpdateDisplayNameResult>
    {
        private const string LogPrefix = nameof(UpdateDisplayNameCommandHandler) + "." + nameof(Handle);

        public async Task<UpdateDisplayNameResult> Handle(UpdateDisplayNameCommand request, CancellationToken cancellationToken)
        {
            using var transaction = transactionLogger.BeginTransaction("UpdateDisplayName", $"Update display name for user {request.UserId}");
            transaction.SetUser(request.UserId.ToString());

            var user = await repository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                logger.LogWarning("{LogPrefix}: user {UserId} not found", LogPrefix, request.UserId);
                transaction.SetCustomAttribute("outcome", "user_not_found");
                return UpdateDisplayNameResult.Rejected("user_not_found");
            }

            if (string.IsNullOrWhiteSpace(request.DisplayName))
            {
                logger.LogWarning("{LogPrefix}: rejected, empty display name for user {UserId}", LogPrefix, request.UserId);
                transaction.SetCustomAttribute("outcome", "display_name_required");
                return UpdateDisplayNameResult.Rejected("display_name_required");
            }

            user.UpdateProfile(user.Email, request.DisplayName, DateTime.UtcNow);
            await repository.SaveChangesAsync(cancellationToken);

            logger.LogInformation("{LogPrefix}: display name updated for user {UserId}", LogPrefix, user.Id);
            return UpdateDisplayNameResult.Updated();
        }
    }
}
