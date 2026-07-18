using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OCRWeb.Identity.Application.Commands.ProvisionGoogleUser;

namespace OCRWeb.API.Endpoints.Internal.GoogleProvision
{
    /// <summary>
    /// POST /internal/identity/google-provision.
    /// Called only by OCRWeb.Bff during the Google OAuth callback — never by Angular directly.
    /// </summary>
    public class GoogleProvisionEndpoint(IMediator mediator) : Endpoint<GoogleProvisionRequest, GoogleProvisionResponse>
    {
        public override void Configure()
        {
            Post("/internal/identity/google-provision");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(GoogleProvisionRequest req, CancellationToken ct)
        {
            var result = await mediator.Send(new ProvisionGoogleUserCommand(req.ProviderKey, req.Email, req.EmailVerified, req.DisplayName), ct);

            await Send.OkAsync(new GoogleProvisionResponse(result.Success, result.UserId, result.RejectionReason), ct);
        }
    }
}
