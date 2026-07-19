using OCRWeb.Bff.Infrastructure.IdentityApiClient;

namespace OCRWeb.Bff.Endpoints
{
    /// <summary>
    /// Maps /bff/admin/users — proxies to OCRWeb.API's internal /internal/identity/users endpoints.
    /// Mapped under the /bff/admin group, which already requires the AdminOnly policy (see Program.cs).
    /// </summary>
    public static class UserAdminEndpoints
    {
        public static void MapUserAdminEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/users", async (IIdentityApiClient client, CancellationToken ct) =>
            {
                var result = await client.ListUsersAsync(ct);
                return Results.Ok(result);
            });

            app.MapPost("/users", async (AddUserRequest req, IIdentityApiClient client, CancellationToken ct) =>
            {
                var result = await client.AddUserAsync(req.Username, req.Email, req.DisplayName, req.TemplateUserId, ct);
                return Results.Ok(result);
            });

            app.MapPut("/users/{id:int}", async (int id, UpdateUserRequest req, IIdentityApiClient client, CancellationToken ct) =>
            {
                var result = await client.UpdateUserAsync(id, req.Email, req.DisplayName, req.IsTemplateUser, req.ParentId, ct);
                return Results.Ok(result);
            });

            app.MapDelete("/users/{id:int}", async (int id, IIdentityApiClient client, CancellationToken ct) =>
            {
                var result = await client.DeleteUserAsync(id, ct);
                return Results.Ok(result);
            });
        }
    }
}
