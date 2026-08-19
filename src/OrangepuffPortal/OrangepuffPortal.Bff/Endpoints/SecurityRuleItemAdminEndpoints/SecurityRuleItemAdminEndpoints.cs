using OrangepuffPortal.Bff.Infrastructure.IdentityGateway;

namespace OrangepuffPortal.Bff.Endpoints.SecurityRuleItemAdminEndpoints
{
    /// <summary>
    /// Maps /bff/admin/security-rule-items onto the Identity module's application layer.
    /// Mapped under the /bff/admin group, which already requires the AdminOnly policy.
    /// </summary>
    public static class SecurityRuleItemAdminEndpoints
    {
        public static void MapSecurityRuleItemAdminEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/security-rule-items", async (int? categoryId, IIdentityGateway client, CancellationToken ct) =>
            {
                var result = await client.ListSecurityRuleItemsAsync(categoryId, ct);
                return Results.Ok(result);
            });

            app.MapPost("/security-rule-items", async (AddSecurityRuleItemRequest req, IIdentityGateway client, CancellationToken ct) =>
            {
                var result = await client.AddSecurityRuleItemAsync(req.CategoryId, req.Code, req.Description, req.RuleType, req.TextCode, req.SortOrder, ct);
                return Results.Ok(result);
            });

            app.MapPut("/security-rule-items/{id:int}", async (int id, UpdateSecurityRuleItemRequest req, IIdentityGateway client, CancellationToken ct) =>
            {
                var result = await client.UpdateSecurityRuleItemAsync(id, req.CategoryId, req.Description, req.RuleType, req.TextCode, req.SortOrder, req.Hidden, ct);
                return Results.Ok(result);
            });

            app.MapDelete("/security-rule-items/{id:int}", async (int id, IIdentityGateway client, CancellationToken ct) =>
            {
                var result = await client.DeleteSecurityRuleItemAsync(id, ct);
                return Results.Ok(result);
            });
        }
    }
}
