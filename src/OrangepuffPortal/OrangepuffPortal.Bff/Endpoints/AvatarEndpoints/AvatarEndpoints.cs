using OrangepuffPortal.Bff.Infrastructure.IdentityGateway;
using System.Security.Claims;

namespace OrangepuffPortal.Bff.Endpoints.AvatarEndpoints
{
    /// <summary>
    /// Maps /bff/me/avatar (self-service upload/clear) and /bff/users/{id}/avatar (view any user's avatar).
    /// </summary>
    public static class AvatarEndpoints
    {
        public static void MapAvatarEndpoints(this WebApplication app)
        {
            app.MapPut("/bff/me/avatar", async (IFormFile? file, ClaimsPrincipal user, IIdentityGateway client, CancellationToken ct) =>
            {
                var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                byte[]? image = null;
                string? contentType = null;
                if (file is not null)
                {
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms, ct);
                    image = ms.ToArray();
                    contentType = file.ContentType;
                }

                var result = await client.UpdateAvatarAsync(userId, image, contentType, ct);
                return Results.Ok(result);
            })
                .RequireAuthorization()
                // Minimal APIs auto-require antiforgery validation for any endpoint binding IFormFile,
                // but this app never wires up app.UseAntiforgery() — every other state-changing route
                // here already relies on the cookie's SameSite=Lax alone for CSRF mitigation, so without
                // this the upload 500s with "contains anti-forgery metadata, but a middleware was not found".
                .DisableAntiforgery();

            app.MapGet("/bff/users/{id:int}/avatar", async (int id, IIdentityGateway client, CancellationToken ct) =>
            {
                var avatar = await client.GetAvatarAsync(id, ct);
                return avatar is null ? Results.NotFound() : Results.Bytes(avatar.Image, avatar.ContentType);
            }).RequireAuthorization();
        }
    }
}
