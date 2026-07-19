using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OCRWeb.Identity.Application.Queries.ListUsers;
using OCRWeb.Identity.Contract;

namespace OCRWeb.API.Endpoints.Internal.ListUsers
{
    /// <summary>
    /// GET /internal/identity/users.
    /// </summary>
    public class ListUsersEndpoint(IMediator mediator) : EndpointWithoutRequest<IReadOnlyList<UserListItemDto>>
    {
        public override void Configure()
        {
            Get("/internal/identity/users");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var result = await mediator.Send(new ListUsersQuery(), ct);
            await Send.OkAsync(result, ct);
        }
    }
}
