using OCRWeb.Bff.Infrastructure.IdentityApiClient;
using System.Security.Claims;

namespace OCRWeb.Bff.Endpoints
{
    /// <summary>
    /// Maps /bff/me/avatar (self-service upload/clear) and /bff/users/{id}/avatar (view any user's avatar).
    /// </summary>
    public static class AvatarEndpoints
    {
        public static void MapAvatarEndpoints(this WebApplication app)
        {
            app.MapPut("/bff/me/avatar", async (IFormFile? file, ClaimsPrincipal user, IIdentityApiClient client, CancellationToken ct) =>
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
            }).RequireAuthorization();

            app.MapGet("/bff/users/{id:int}/avatar", async (int id, IIdentityApiClient client, CancellationToken ct) =>
            {
                var avatar = await client.GetAvatarAsync(id, ct);
                return avatar is null ? Results.NotFound() : Results.Bytes(avatar.Image, avatar.ContentType);
            }).RequireAuthorization();
        }
    }
}
