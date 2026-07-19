using OCRWeb.Bff.Infrastructure.IdentityApiClient;

namespace OCRWeb.Bff.Endpoints
{
    /// <summary>
    /// Maps /bff/admin/security-rule-categories — proxies to OCRWeb.API's internal
    /// /internal/identity/security-rule-categories endpoints.
    /// Mapped under the /bff/admin group, which already requires the AdminOnly policy (see Program.cs).
    /// </summary>
    public static class SecurityRuleCategoryAdminEndpoints
    {
        public static void MapSecurityRuleCategoryAdminEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/security-rule-categories", async (IIdentityApiClient client, CancellationToken ct) =>
            {
                var result = await client.ListSecurityRuleCategoriesAsync(ct);
                return Results.Ok(result);
            });

            app.MapPost("/security-rule-categories", async (AddSecurityRuleCategoryRequest req, IIdentityApiClient client, CancellationToken ct) =>
            {
                var result = await client.AddSecurityRuleCategoryAsync(req.CategoryDesc, req.TextCode, ct);
                return Results.Ok(result);
            });

            app.MapPut("/security-rule-categories/{id:int}", async (int id, UpdateSecurityRuleCategoryRequest req, IIdentityApiClient client, CancellationToken ct) =>
            {
                var result = await client.UpdateSecurityRuleCategoryAsync(id, req.CategoryDesc, req.TextCode, req.Hidden, ct);
                return Results.Ok(result);
            });

            app.MapDelete("/security-rule-categories/{id:int}", async (int id, IIdentityApiClient client, CancellationToken ct) =>
            {
                var result = await client.DeleteSecurityRuleCategoryAsync(id, ct);
                return Results.Ok(result);
            });
        }
    }
}
