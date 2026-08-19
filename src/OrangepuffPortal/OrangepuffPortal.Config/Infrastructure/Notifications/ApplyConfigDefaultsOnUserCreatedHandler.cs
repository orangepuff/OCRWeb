using MediatR;
using Microsoft.Extensions.Logging;
using OrangepuffPortal.Config.Contract.Interfaces;
using OrangepuffPortal.Shared.Events;

namespace OrangepuffPortal.Config.Infrastructure.Notifications;

/// <summary>
/// Reacts to a new user (admin-added or Google self-registration, see <see cref="UserCreatedNotification"/>)
/// by applying every config's current default value to them.
/// </summary>
/// <remarks>
/// Deliberately swallows any exception rather than rethrowing — this runs synchronously as part of the
/// user-creation request/sign-in that published the notification, and a hiccup applying config defaults
/// must never surface as "user creation failed" when the user was already created successfully.
/// </remarks>
internal class ApplyConfigDefaultsOnUserCreatedHandler(
    IConfigUserValueService configUserValueService,
    ILogger<ApplyConfigDefaultsOnUserCreatedHandler> logger) : INotificationHandler<UserCreatedNotification>
{
    private const string LogPrefix = nameof(ApplyConfigDefaultsOnUserCreatedHandler) + "." + nameof(Handle);

    public async Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            await configUserValueService.ApplyDefaultsForNewUserAsync(notification.UserId, notification.ActorUserId, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "{LogPrefix}: failed to apply config defaults for new user {UserId}", LogPrefix, notification.UserId);
        }
    }
}
