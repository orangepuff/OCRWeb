using OrangepuffPortal.Bff.Infrastructure.IdentityGateway;

namespace OrangepuffPortal.Bff.Endpoints.AvatarAdminEndpoints
{
    /// <summary>
    /// Maps /bff/admin/users/{id}/avatar — admin upload/clear of any user's avatar.
    /// Mapped under the /bff/admin group, which already requires the AdminOnly policy.
    /// </summary>
    public static class AvatarAdminEndpoints
    {
        public static void MapAvatarAdminEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPut("/users/{id:int}/avatar", async (int id, IFormFile? file, IIdentityGateway client, CancellationToken ct) =>
            {
                byte[]? image = null;
                string? contentType = null;
                if (file is not null)
                {
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms, ct);
                    image = ms.ToArray();
                    contentType = file.ContentType;
                }

                var result = await client.UpdateAvatarAsync(id, image, contentType, ct);
                return Results.Ok(result);
            }).DisableAntiforgery(); // see AvatarEndpoints.cs — IFormFile binding auto-requires antiforgery, which this app never wires up.
        }
    }
}
