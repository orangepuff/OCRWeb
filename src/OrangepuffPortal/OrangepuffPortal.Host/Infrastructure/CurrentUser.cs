using System.Security.Claims;
using OrangepuffPortal.Identity.Domain.Entity;
using OrangepuffPortal.Shared.Auditing;

namespace OrangepuffPortal.Host.Infrastructure;

/// <summary>
/// Reads the current user id from the signed-in cookie principal's NameIdentifier claim.
/// </summary>
public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public int UserId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var id)
                ? id
                : throw new InvalidOperationException("No authenticated user id claim present. Ensure the endpoint requires authentication.");
        }
    }

    // Falls back to the default culture (rather than throwing like UserId does) so sessions signed in
    // before this claim existed keep working until they next re-authenticate.
    public string CultureCode =>
        httpContextAccessor.HttpContext?.User?.FindFirstValue(PortalClaimTypes.CultureCode) ?? User.DefaultCultureCode;
}
