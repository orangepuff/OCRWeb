using System.Security.Claims;
using OCRWeb.Shared.Auditing;

namespace OCRWeb.API.Infrastructure;

/// <summary>
/// Reads the current user id from the validated JWT's NameIdentifier claim.
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
}
