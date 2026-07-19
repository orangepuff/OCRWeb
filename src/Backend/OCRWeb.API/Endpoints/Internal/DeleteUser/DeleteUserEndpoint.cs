using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OCRWeb.Identity.Application.Commands.DeleteUser;

namespace OCRWeb.API.Endpoints.Internal.DeleteUser
{
    /// <summary>
    /// DELETE /internal/identity/users/{id}.
    /// </summary>
    public class DeleteUserEndpoint(IMediator mediator) : Endpoint<DeleteUserRequest, DeleteUserResponse>
    {
        public override void Configure()
        {
            Delete("/internal/identity/users/{id}");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(DeleteUserRequest req, CancellationToken ct)
        {
            var result = await mediator.Send(new DeleteUserCommand(req.Id), ct);

            await Send.OkAsync(new DeleteUserResponse(result.Success, result.RejectionReason), ct);
        }
    }
}
