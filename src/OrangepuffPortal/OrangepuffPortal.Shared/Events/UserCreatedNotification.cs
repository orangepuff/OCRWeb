using MediatR;

namespace OrangepuffPortal.Shared.Events;

/// <summary>
/// Published (via MediatR) after a new user is created — by <c>AddUserCommandHandler</c> (admin-added)
/// and <c>ProvisionGoogleUserCommandHandler</c> (self-registration), both in
/// <c>OrangepuffPortal.Identity</c>. Lives here rather than in either module because Identity and Config
/// don't otherwise reference each other; this is the one deliberate cross-module integration point.
/// </summary>
/// <param name="UserId">The newly created user's id.</param>
/// <param name="ActorUserId">
/// The acting admin's id for an admin-added user, or 0 ("system" — see ConfigConstants.SystemActorUserId
/// in OrangepuffPortal.Config) when there's no real actor, e.g. Google self-registration.
/// </param>
public sealed record UserCreatedNotification(int UserId, int ActorUserId) : INotification;
