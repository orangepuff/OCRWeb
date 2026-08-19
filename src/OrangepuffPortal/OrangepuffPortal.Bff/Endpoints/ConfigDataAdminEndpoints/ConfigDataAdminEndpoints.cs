using OrangepuffPortal.Bff.Infrastructure.ConfigDataGateway;
using OrangepuffPortal.ConfigData.Contract;
using System.Security.Claims;

namespace OrangepuffPortal.Bff.Endpoints.ConfigDataAdminEndpoints
{
    /// <summary>
    /// Maps /bff/admin/config-data onto the ConfigData module's admin service.
    /// Mapped under the /bff/admin group, which already requires the AdminOnly policy.
    /// </summary>
    public static class ConfigDataAdminEndpoints
    {
        public static void MapConfigDataAdminEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/config-data", async (IConfigDataGateway gateway, CancellationToken ct) =>
            {
                var result = await gateway.GetAllAsync(ct);
                return Results.Ok(result);
            });

            app.MapPost("/config-data", async (CreateConfigDataRequest req, ClaimsPrincipal user, IConfigDataGateway gateway, CancellationToken ct) =>
            {
                var actorUserId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var result = await gateway.CreateAsync(new ConfigDataUpsertRequest(req.SKey, req.SValue, req.BAllowEditByScreen, req.SDescription), actorUserId, ct);
                return Results.Ok(result);
            });

            app.MapPut("/config-data/{id:int}", async (int id, UpdateConfigDataRequest req, ClaimsPrincipal user, IConfigDataGateway gateway, CancellationToken ct) =>
            {
                var actorUserId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var result = await gateway.UpdateAsync(id, new ConfigDataUpsertRequest(req.SKey, req.SValue, req.BAllowEditByScreen, req.SDescription), actorUserId, ct);
                return Results.Ok(result);
            });

            app.MapDelete("/config-data/{id:int}", async (int id, IConfigDataGateway gateway, CancellationToken ct) =>
            {
                var result = await gateway.DeleteAsync(id, ct);
                return Results.Ok(result);
            });
        }
    }

    internal sealed record CreateConfigDataRequest(string SKey, string? SValue, bool? BAllowEditByScreen, string? SDescription);

    internal sealed record UpdateConfigDataRequest(string SKey, string? SValue, bool? BAllowEditByScreen, string? SDescription);
}
