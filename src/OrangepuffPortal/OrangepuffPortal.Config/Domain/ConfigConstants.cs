namespace OrangepuffPortal.Config.Domain;

/// <summary>
/// <c>identity.Users.iId</c> is an IDENTITY starting at 1, so 0 can never collide with a real user —
/// it marks a <c>ConfigUsers</c> row as applied by unattended system code (seed-time backfill, Google
/// self-registration) rather than a real signed-in actor.
/// </summary>
internal static class ConfigConstants
{
    public const int SystemActorUserId = 0;
}
