using Diagnostics.Abstractions.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrangepuffPortal.Identity.Domain.Entity;
using OrangepuffPortal.Identity.Domain.Repositories;
using OrangepuffPortal.Identity.Infrastructure.Seeding;

namespace OrangepuffPortal.Identity.Application.Commands.VerifyPassword
{
    public class VerifyPasswordCommandHandler(
        IUserRepository repository,
        IPasswordHasher<User> passwordHasher,
        IHostEnvironment environment,
        IOptions<SeedOptions> seedOptions,
        ITransactionLogger transactionLogger,
        ILogger<VerifyPasswordCommandHandler> logger) : IRequestHandler<VerifyPasswordCommand, VerifyPasswordResult>
    {
        private const string LogPrefix = nameof(VerifyPasswordCommandHandler) + "." + nameof(Handle);

        public async Task<VerifyPasswordResult> Handle(VerifyPasswordCommand request, CancellationToken cancellationToken)
        {
            using var transaction = transactionLogger.BeginTransaction("VerifyPassword", $"Password sign-in attempt for {request.UsernameOrEmail}");

            var user = await repository.GetByUsernameAsync(request.UsernameOrEmail, cancellationToken)
                ?? await repository.GetByEmailAsync(request.UsernameOrEmail, cancellationToken);

            if (user is null)
            {
                logger.LogWarning("{LogPrefix}: rejected, no matching account for {UsernameOrEmail}", LogPrefix, request.UsernameOrEmail);
                transaction.SetCustomAttribute("outcome", "invalid_credentials");
                return VerifyPasswordResult.Rejected("invalid_credentials");
            }

            transaction.SetUser(user.Id.ToString());

            if (!user.IsActive)
            {
                logger.LogWarning("{LogPrefix}: rejected, user {UserId} is inactive", LogPrefix, user.Id);
                transaction.SetCustomAttribute("outcome", "account_inactive");
                return VerifyPasswordResult.Rejected("account_inactive");
            }

            // DEV bypass runs before hash check so it works even when the stored hash is stale or absent.
            if (IsDevAdminBypass(user, request.Password))
            {
                logger.LogWarning("{LogPrefix}: DEV bypass used for user {UserId} — never enable in production", LogPrefix, user.Id);
                transaction.SetCustomAttribute("outcome", "dev_bypass");
                return VerifyPasswordResult.Allowed(user.Id, user.Email, user.DisplayName, user.CultureCode);
            }

            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                logger.LogWarning("{LogPrefix}: rejected, account {UserId} has no password set", LogPrefix, user.Id);
                transaction.SetCustomAttribute("outcome", "invalid_credentials");
                return VerifyPasswordResult.Rejected("invalid_credentials");
            }

            var verification = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (verification == PasswordVerificationResult.Failed)
            {
                logger.LogWarning("{LogPrefix}: rejected, wrong password for user {UserId}", LogPrefix, user.Id);
                transaction.SetCustomAttribute("outcome", "invalid_credentials");
                return VerifyPasswordResult.Rejected("invalid_credentials");
            }

            if (verification == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.SetPasswordHash(passwordHasher.HashPassword(user, request.Password), DateTime.UtcNow);
                await repository.SaveChangesAsync(cancellationToken);
            }

            logger.LogInformation("{LogPrefix}: verified password sign-in for user {UserId}", LogPrefix, user.Id);
            return VerifyPasswordResult.Allowed(user.Id, user.Email, user.DisplayName, user.CultureCode);
        }

        /// <summary>
        /// DEV-only: lets the seed admin account log in with the plain-text seed password even
        /// if the DB hash is stale or missing. Active only when the host environment is Development
        /// and <see cref="SeedOptions.AdminPassword"/> is non-empty.
        /// </summary>
        private bool IsDevAdminBypass(User user, string providedPassword)
        {
            if (!environment.IsDevelopment())
            {
                return false;
            }

            var seed = seedOptions.Value;
            if (string.IsNullOrEmpty(seed.AdminPassword))
            {
                return false;
            }

            return string.Equals(user.Username, seed.AdminUsername, StringComparison.OrdinalIgnoreCase)
                && string.Equals(providedPassword, seed.AdminPassword, StringComparison.Ordinal);
        }
    }
}
