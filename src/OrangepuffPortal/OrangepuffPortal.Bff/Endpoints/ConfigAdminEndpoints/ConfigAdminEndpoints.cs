using OrangepuffPortal.Bff.Infrastructure.ConfigGateway;
using OrangepuffPortal.Config.Contract;
using System.Security.Claims;

namespace OrangepuffPortal.Bff.Endpoints.ConfigAdminEndpoints
{
    /// <summary>
    /// Maps /bff/admin/users/{userId}/config/{configCode} (per-user values) and /bff/admin/config/*
    /// (catalog CRUD) onto the Config module. Mapped under the /bff/admin group, which already
    /// requires the AdminOnly policy — writes are admin-only regardless of Configs.btAllowUserEdit
    /// (see docs/config-settings-design.md).
    /// </summary>
    public static class ConfigAdminEndpoints
    {
        public static void MapConfigAdminEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPut("/users/{userId:int}/config/{configCode}", async (int userId, string configCode, SetConfigValueRequest req, IConfigGateway gateway, CancellationToken ct) =>
            {
                await gateway.SetConfigValueAsync(userId, configCode, req.Value, ct);
                return Results.NoContent();
            });

            app.MapGet("/config/sections", async (IConfigGateway gateway, CancellationToken ct) =>
            {
                var result = await gateway.ListSectionsAsync(ct);
                return Results.Ok(result);
            });

            app.MapPost("/config/sections", async (AddConfigSectionRequest req, ClaimsPrincipal user, IConfigGateway gateway, CancellationToken ct) =>
            {
                var actorUserId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var result = await gateway.AddSectionAsync(new ConfigSectionUpsertRequest(req.SSectionDesc, req.STextCode, req.BtShow, req.ISortOrder), actorUserId, ct);
                return Results.Ok(result);
            });

            app.MapPut("/config/sections/{id:int}", async (int id, UpdateConfigSectionRequest req, ClaimsPrincipal user, IConfigGateway gateway, CancellationToken ct) =>
            {
                var actorUserId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var result = await gateway.UpdateSectionAsync(id, new ConfigSectionUpsertRequest(req.SSectionDesc, req.STextCode, req.BtShow, req.ISortOrder), actorUserId, ct);
                return Results.Ok(result);
            });

            app.MapDelete("/config/sections/{id:int}", async (int id, IConfigGateway gateway, CancellationToken ct) =>
            {
                var result = await gateway.DeleteSectionAsync(id, ct);
                return Results.Ok(result);
            });

            app.MapGet("/config/items", async (int? sectionId, string? configCode, string? configName, int? configType, string? sortBy, bool? sortDescending, int? page, int? pageSize, IConfigGateway gateway, CancellationToken ct) =>
            {
                var result = await gateway.ListConfigsAsync(sectionId, configCode, configName, configType, sortBy, sortDescending ?? false, page ?? 1, pageSize ?? 50, ct);
                return Results.Ok(result);
            });

            app.MapPost("/config/items", async (AddConfigItemRequest req, ClaimsPrincipal user, IConfigGateway gateway, CancellationToken ct) =>
            {
                var actorUserId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var result = await gateway.AddConfigAsync(
                    new ConfigItemUpsertRequest(req.ISectionId, req.SConfigCode, req.SConfigName, req.STextCode, req.IConfigType, req.BtShow, req.BtAllowUserEdit, req.ISortOrder, req.SDefaultValue, req.IDefaultValue, req.NDefaultValue, req.BtDefaultValue),
                    actorUserId, ct);
                return Results.Ok(result);
            });

            app.MapPut("/config/items/{id:int}", async (int id, UpdateConfigItemRequest req, ClaimsPrincipal user, IConfigGateway gateway, CancellationToken ct) =>
            {
                var actorUserId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var result = await gateway.UpdateConfigAsync(id,
                    new ConfigItemUpsertRequest(req.ISectionId, req.SConfigCode, req.SConfigName, req.STextCode, req.IConfigType, req.BtShow, req.BtAllowUserEdit, req.ISortOrder, req.SDefaultValue, req.IDefaultValue, req.NDefaultValue, req.BtDefaultValue),
                    actorUserId, ct);
                return Results.Ok(result);
            });

            app.MapDelete("/config/items/{id:int}", async (int id, IConfigGateway gateway, CancellationToken ct) =>
            {
                var result = await gateway.DeleteConfigAsync(id, ct);
                return Results.Ok(result);
            });
        }
    }
}
