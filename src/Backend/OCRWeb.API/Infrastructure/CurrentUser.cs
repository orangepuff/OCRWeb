using System.Security.Claims;
using OCRWeb.Shared.Auditing;

namespace OCRWeb.API.Infrastructure;

/// <summary>
/// Stub implementation of <see cref="ICurrentUser"/> until authentication is added.
/// Reads the user id from claims if present; otherwise falls back to the seeded admin (id 1).
/// </summary>
public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private const int SeededAdminUserId = 1;

    public int UserId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var id) ? id : SeededAdminUserId;
        }
    }
}
