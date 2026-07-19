using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OCRWeb.Identity.Application.Commands.UpdateUser;

namespace OCRWeb.API.Endpoints.Internal.UpdateUser
{
    /// <summary>
    /// PUT /internal/identity/users/{id}.
    /// </summary>
    public class UpdateUserEndpoint(IMediator mediator) : Endpoint<UpdateUserRequest, UpdateUserResponse>
    {
        public override void Configure()
        {
            Put("/internal/identity/users/{id}");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(UpdateUserRequest req, CancellationToken ct)
        {
            var result = await mediator.Send(new UpdateUserCommand(req.Id, req.Email, req.DisplayName, req.IsTemplateUser, req.ParentId), ct);

            await Send.OkAsync(new UpdateUserResponse(result.Success, result.RejectionReason), ct);
        }
    }
}
