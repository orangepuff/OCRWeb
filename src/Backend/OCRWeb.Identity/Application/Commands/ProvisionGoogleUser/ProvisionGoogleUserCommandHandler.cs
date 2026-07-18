using Diagnostics.Abstractions.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using OCRWeb.Identity.Domain.Entity;
using OCRWeb.Identity.Domain.Repositories;

namespace OCRWeb.Identity.Application.Commands.ProvisionGoogleUser
{
    public class ProvisionGoogleUserCommandHandler(
        IUserRepository repository
        , IUserRegistrationPolicy registrationPolicy
        , ITransactionLogger transactionLogger
        , ILogger<ProvisionGoogleUserCommandHandler> logger) : IRequestHandler<ProvisionGoogleUserCommand, ProvisionGoogleUserResult>
    {
        private const string GoogleProvider = "Google";
        private const string LogPrefix = nameof(ProvisionGoogleUserCommandHandler) + "." + nameof(Handle);

        public async Task<ProvisionGoogleUserResult> Handle(ProvisionGoogleUserCommand request, CancellationToken cancellationToken)
        {
            using var transaction = transactionLogger.BeginTransaction("ProvisionGoogleUser", $"Google sign-in for {request.Email}");

            var existingLinkedUser = await repository.GetByExternalLoginAsync(GoogleProvider, request.ProviderKey, cancellationToken);
            if (existingLinkedUser is not null)
            {
                transaction.SetUser(existingLinkedUser.Id.ToString());
                logger.LogInformation("{LogPrefix}: resolved existing linked user {UserId}", LogPrefix, existingLinkedUser.Id);
                return ProvisionGoogleUserResult.Allowed(existingLinkedUser.Id);
            }

            if (!request.EmailVerified)
            {
                logger.LogWarning("{LogPrefix}: rejected sign-in for {Email}, email not verified", LogPrefix, request.Email);
                transaction.SetCustomAttribute("outcome", "email_not_verified");
                return ProvisionGoogleUserResult.Rejected("email_not_verified");
            }

            var now = DateTime.UtcNow;
            var existingUserByEmail = await repository.GetByEmailAsync(request.Email, cancellationToken);
            if (existingUserByEmail is not null)
            {
                await repository.AddExternalLoginAsync(new ExternalLogin(existingUserByEmail.Id, GoogleProvider, request.ProviderKey, now), cancellationToken);
                await repository.SaveChangesAsync(cancellationToken);

                transaction.SetUser(existingUserByEmail.Id.ToString());
                logger.LogInformation("{LogPrefix}: linked Google account to existing user {UserId}", LogPrefix, existingUserByEmail.Id);
                return ProvisionGoogleUserResult.Allowed(existingUserByEmail.Id);
            }

            if (!await registrationPolicy.IsSelfRegistrationAllowedAsync(cancellationToken))
            {
                logger.LogWarning("{LogPrefix}: rejected sign-in for {Email}, self-registration disabled", LogPrefix, request.Email);
                transaction.SetCustomAttribute("outcome", "registration_disabled");

                return ProvisionGoogleUserResult.Rejected("registration_disabled");
            }

            var username = $"google_{request.Email.Split('@')[0]}";
            var newUser = new User(username, request.Email, request.DisplayName, now);
            await repository.AddAsync(newUser, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);

            await repository.AddExternalLoginAsync(new ExternalLogin(newUser.Id, GoogleProvider, request.ProviderKey, now), cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);

            transaction.SetUser(newUser.Id.ToString());
            logger.LogInformation("{LogPrefix}: provisioned new user {UserId} from Google sign-in", LogPrefix, newUser.Id);
            return ProvisionGoogleUserResult.Allowed(newUser.Id);
        }
    }
}
