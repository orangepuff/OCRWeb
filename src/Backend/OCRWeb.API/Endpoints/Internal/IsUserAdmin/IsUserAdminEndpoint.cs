using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OCRWeb.Identity.Application.Queries.IsUserAdmin;

namespace OCRWeb.API.Endpoints.Internal.IsUserAdmin
{
    /// <summary>
    /// GET /internal/identity/users/{id}/is-admin.
    /// </summary>
    public class IsUserAdminEndpoint(IMediator mediator) : Endpoint<IsUserAdminRequest, IsUserAdminResponse>
    {
        public override void Configure()
        {
            Get("/internal/identity/users/{id}/is-admin");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(IsUserAdminRequest req, CancellationToken ct)
        {
            var isAdmin = await mediator.Send(new IsUserAdminQuery(req.Id), ct);
            await Send.OkAsync(new IsUserAdminResponse(isAdmin), ct);
        }
    }
}
