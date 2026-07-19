using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OCRWeb.Identity.Application.Commands.AddUser;

namespace OCRWeb.API.Endpoints.Internal.AddUser
{
    /// <summary>
    /// POST /internal/identity/users.
    /// </summary>
    public class AddUserEndpoint(IMediator mediator) : Endpoint<AddUserRequest, AddUserResponse>
    {
        public override void Configure()
        {
            Post("/internal/identity/users");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(AddUserRequest req, CancellationToken ct)
        {
            var result = await mediator.Send(new AddUserCommand(req.Username, req.Email, req.DisplayName, req.TemplateUserId), ct);

            await Send.OkAsync(new AddUserResponse(result.Success, result.UserId, result.RejectionReason), ct);
        }
    }
}
