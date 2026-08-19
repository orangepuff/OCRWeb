using OrangepuffPortal.Bff.Infrastructure.ConfigTextGateway;
using OrangepuffPortal.ConfigText.Contract;
using System.Security.Claims;

namespace OrangepuffPortal.Bff.Endpoints.ConfigTextAdminEndpoints
{
    /// <summary>
    /// Maps /bff/admin/config-text onto the ConfigText module's admin service.
    /// Mapped under the /bff/admin group, which already requires the AdminOnly policy.
    /// </summary>
    public static class ConfigTextAdminEndpoints
    {
        public static void MapConfigTextAdminEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/config-text", async (
                string? module, string? textCode, string? cultureCode, string? textType, string? text, int? page, int? pageSize,
                IConfigTextGateway gateway, CancellationToken ct) =>
            {
                var result = await gateway.ListAsync(module, textCode, cultureCode, textType, text, page ?? 1, pageSize ?? 50, ct);
                return Results.Ok(result);
            });

            app.MapPost("/config-text", async (AddConfigTextRequest req, ClaimsPrincipal user, IConfigTextGateway gateway, CancellationToken ct) =>
            {
                var actorUserId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var result = await gateway.AddAsync(new ConfigTextAdminUpsertRequest(req.SModule, req.STextCode, req.SCultureCode, req.STextType, req.SText, req.SNote), actorUserId, ct);
                return Results.Ok(result);
            });

            app.MapPut("/config-text/{id:int}", async (int id, UpdateConfigTextRequest req, ClaimsPrincipal user, IConfigTextGateway gateway, CancellationToken ct) =>
            {
                var actorUserId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var result = await gateway.UpdateAsync(id, new ConfigTextAdminUpsertRequest(req.SModule, req.STextCode, req.SCultureCode, req.STextType, req.SText, req.SNote), actorUserId, ct);
                return Results.Ok(result);
            });

            app.MapDelete("/config-text/{id:int}", async (int id, IConfigTextGateway gateway, CancellationToken ct) =>
            {
                var result = await gateway.DeleteAsync(id, ct);
                return Results.Ok(result);
            });
        }
    }
}
