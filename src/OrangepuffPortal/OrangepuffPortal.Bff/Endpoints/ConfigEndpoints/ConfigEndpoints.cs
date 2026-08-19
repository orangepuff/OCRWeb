using OrangepuffPortal.Bff.Infrastructure;
using OrangepuffPortal.Bff.Infrastructure.ConfigGateway;
using System.Security.Claims;

namespace OrangepuffPortal.Bff.Endpoints.ConfigEndpoints
{
    /// <summary>
    /// Maps /bff/users/{userId}/config — self-or-admin read of a user's config catalog + values.
    /// </summary>
    public static class ConfigEndpoints
    {
        public static void MapConfigEndpoints(this WebApplication app)
        {
            app.MapGet("/bff/users/{userId:int}/config", async (int userId, ClaimsPrincipal user, IConfigGateway gateway, CancellationToken ct) =>
            {
                var callerId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var isAdmin = user.HasClaim(PortalBffConstants.AdminClaimType, "true");

                if (callerId != userId && !isAdmin)
                {
                    return Results.Forbid();
                }

                var sections = await gateway.GetSectionsForUserAsync(userId, ct);
                return Results.Ok(sections);
            }).RequireAuthorization();
        }
    }
}
