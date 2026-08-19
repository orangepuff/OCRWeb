using OrangepuffPortal.Bff.Infrastructure.IdentityGateway;

namespace OrangepuffPortal.Bff.Endpoints.SecurityRuleCategoryAdminEndpoints
{
    /// <summary>
    /// Maps /bff/admin/security-rule-categories onto the Identity module's application layer.
    /// Mapped under the /bff/admin group, which already requires the AdminOnly policy.
    /// </summary>
    public static class SecurityRuleCategoryAdminEndpoints
    {
        public static void MapSecurityRuleCategoryAdminEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/security-rule-categories", async (IIdentityGateway client, CancellationToken ct) =>
            {
                var result = await client.ListSecurityRuleCategoriesAsync(ct);
                return Results.Ok(result);
            });

            app.MapPost("/security-rule-categories", async (AddSecurityRuleCategoryRequest req, IIdentityGateway client, CancellationToken ct) =>
            {
                var result = await client.AddSecurityRuleCategoryAsync(req.CategoryDesc, req.TextCode, ct);
                return Results.Ok(result);
            });

            app.MapPut("/security-rule-categories/{id:int}", async (int id, UpdateSecurityRuleCategoryRequest req, IIdentityGateway client, CancellationToken ct) =>
            {
                var result = await client.UpdateSecurityRuleCategoryAsync(id, req.CategoryDesc, req.TextCode, req.Hidden, ct);
                return Results.Ok(result);
            });

            app.MapDelete("/security-rule-categories/{id:int}", async (int id, IIdentityGateway client, CancellationToken ct) =>
            {
                var result = await client.DeleteSecurityRuleCategoryAsync(id, ct);
                return Results.Ok(result);
            });
        }
    }
}
