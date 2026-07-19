using OCRWeb.Bff.Infrastructure.IdentityApiClient;

namespace OCRWeb.Bff.Endpoints
{
    /// <summary>
    /// Maps /bff/admin/security-rule-categories — proxies to OCRWeb.API's internal
    /// /internal/identity/security-rule-categories endpoints.
    /// </summary>
    public static class SecurityRuleCategoryAdminEndpoints
    {
        public static void MapSecurityRuleCategoryAdminEndpoints(this WebApplication app)
        {
            app.MapGet("/bff/admin/security-rule-categories", async (IIdentityApiClient client, CancellationToken ct) =>
            {
                var result = await client.ListSecurityRuleCategoriesAsync(ct);
                return Results.Ok(result);
            }).RequireAuthorization();

            app.MapPost("/bff/admin/security-rule-categories", async (AddSecurityRuleCategoryRequest req, IIdentityApiClient client, CancellationToken ct) =>
            {
                var result = await client.AddSecurityRuleCategoryAsync(req.CategoryDesc, req.TextCode, ct);
                return Results.Ok(result);
            }).RequireAuthorization();

            app.MapPut("/bff/admin/security-rule-categories/{id:int}", async (int id, UpdateSecurityRuleCategoryRequest req, IIdentityApiClient client, CancellationToken ct) =>
            {
                var result = await client.UpdateSecurityRuleCategoryAsync(id, req.CategoryDesc, req.TextCode, req.Hidden, ct);
                return Results.Ok(result);
            }).RequireAuthorization();

            app.MapDelete("/bff/admin/security-rule-categories/{id:int}", async (int id, IIdentityApiClient client, CancellationToken ct) =>
            {
                var result = await client.DeleteSecurityRuleCategoryAsync(id, ct);
                return Results.Ok(result);
            }).RequireAuthorization();
        }
    }
}
