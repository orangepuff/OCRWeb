using OrangepuffPortal.Bff.Infrastructure.IdentityGateway;
using System.Security.Claims;

namespace OrangepuffPortal.Bff.Endpoints.UserAdminEndpoints
{
    /// <summary>
    /// Maps /bff/admin/users onto the Identity module's application layer.
    /// Mapped under the /bff/admin group, which already requires the AdminOnly policy.
    /// </summary>
    public static class UserAdminEndpoints
    {
        public static void MapUserAdminEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/users", async (IIdentityGateway client, CancellationToken ct) =>
            {
                var result = await client.ListUsersAsync(ct);
                return Results.Ok(result);
            });

            app.MapPost("/users", async (AddUserRequest req, ClaimsPrincipal user, IIdentityGateway client, CancellationToken ct) =>
            {
                var actorUserId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var result = await client.AddUserAsync(req.Username, req.Email, req.DisplayName, req.TemplateUserId, req.Password, actorUserId, ct);
                return Results.Ok(result);
            });

            app.MapPut("/users/{id:int}", async (int id, UpdateUserRequest req, IIdentityGateway client, CancellationToken ct) =>
            {
                var result = await client.UpdateUserAsync(id, req.Email, req.DisplayName, req.IsActive, req.IsTemplateUser, req.ParentId, ct);
                return Results.Ok(result);
            });

            app.MapPut("/users/{id:int}/password", async (int id, SetPasswordRequest req, IIdentityGateway client, CancellationToken ct) =>
            {
                var result = await client.SetUserPasswordAsync(id, req.NewPassword, ct);
                return Results.Ok(result);
            });

            app.MapDelete("/users/{id:int}", async (int id, IIdentityGateway client, CancellationToken ct) =>
            {
                var result = await client.DeleteUserAsync(id, ct);
                return Results.Ok(result);
            });
        }
    }
}
