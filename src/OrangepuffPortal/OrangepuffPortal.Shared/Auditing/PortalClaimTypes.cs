namespace OrangepuffPortal.Shared.Auditing;

/// <summary>
/// Claim types Portal's auth cookie carries beyond the built-in <see cref="System.Security.Claims.ClaimTypes"/>.
/// Public (unlike Bff-internal claim types such as the admin/lv claims) because <see cref="ICurrentUser"/>
/// implementations outside the Bff project need to read them off the signed-in principal.
/// </summary>
public static class PortalClaimTypes
{
    /// <summary>Culture code of the signed-in user (e.g. "en-US").</summary>
    public const string CultureCode = "culture";
}
