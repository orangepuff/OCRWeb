using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OCRWeb.Identity.Application.Queries.GetUserAvatar;

namespace OCRWeb.API.Endpoints.Internal.GetUserAvatar
{
    /// <summary>
    /// GET /internal/identity/users/{id}/avatar — stream a user's profile picture.
    /// </summary>
    public class GetUserAvatarEndpoint(IMediator mediator) : Endpoint<GetUserAvatarRequest>
    {
        public override void Configure()
        {
            Get("/internal/identity/users/{id}/avatar");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(GetUserAvatarRequest req, CancellationToken ct)
        {
            var avatar = await mediator.Send(new GetUserAvatarQuery(req.Id), ct);
            if (avatar is null)
            {
                await Send.NotFoundAsync(ct);
                return;
            }

            await Send.BytesAsync(avatar.Image, contentType: avatar.ContentType, cancellation: ct);
        }
    }
}
