using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OCRWeb.Identity.Application.Queries.IsUserActive;

namespace OCRWeb.API.Endpoints.Internal.IsUserActive
{
    /// <summary>
    /// GET /internal/identity/users/{id}/is-active.
    /// Called only by OCRWeb.Bff's periodic session revalidation — never by Angular directly.
    /// </summary>
    public class IsUserActiveEndpoint(IMediator mediator) : Endpoint<IsUserActiveRequest, IsUserActiveResponse>
    {
        public override void Configure()
        {
            Get("/internal/identity/users/{id}/is-active");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(IsUserActiveRequest req, CancellationToken ct)
        {
            var isActive = await mediator.Send(new IsUserActiveQuery(req.Id), ct);
            await Send.OkAsync(new IsUserActiveResponse(isActive), ct);
        }
    }
}
