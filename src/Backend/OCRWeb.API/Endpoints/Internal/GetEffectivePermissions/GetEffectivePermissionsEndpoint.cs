using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OCRWeb.Identity.Application.Queries.GetEffectivePermissions;
using OCRWeb.Identity.Contract;

namespace OCRWeb.API.Endpoints.Internal.GetEffectivePermissions
{
    /// <summary>
    /// GET /internal/identity/users/{id}/permissions.
    /// Resolved through template-user inheritance — callers never need to know whether
    /// the user has their own SecurityUserRuleItems rows or inherits from a template.
    /// </summary>
    public class GetEffectivePermissionsEndpoint(IMediator mediator) : Endpoint<GetEffectivePermissionsRequest, IReadOnlyList<EffectivePermissionDto>>
    {
        public override void Configure()
        {
            Get("/internal/identity/users/{id}/permissions");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(GetEffectivePermissionsRequest req, CancellationToken ct)
        {
            var result = await mediator.Send(new GetEffectivePermissionsQuery(req.Id), ct);
            await Send.OkAsync(result, ct);
        }
    }
}
