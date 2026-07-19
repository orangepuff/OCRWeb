using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OCRWeb.Identity.Application.Commands.UpdateUserAvatar;

namespace OCRWeb.API.Endpoints.Internal.UpdateUserAvatar
{
    /// <summary>
    /// PUT /internal/identity/users/{id}/avatar. No file in the multipart body clears the avatar.
    /// </summary>
    public class UpdateUserAvatarEndpoint(IMediator mediator) : Endpoint<UpdateUserAvatarRequest, UpdateUserAvatarResponse>
    {
        public override void Configure()
        {
            Put("/internal/identity/users/{id}/avatar");
            AllowFileUploads();
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(UpdateUserAvatarRequest req, CancellationToken ct)
        {
            byte[]? image = null;
            string? contentType = null;

            if (req.File is not null)
            {
                using var ms = new MemoryStream();
                await req.File.CopyToAsync(ms, ct);
                image = ms.ToArray();
                contentType = string.IsNullOrWhiteSpace(req.File.ContentType) ? "application/octet-stream" : req.File.ContentType;
            }

            var result = await mediator.Send(new UpdateUserAvatarCommand(req.Id, image, contentType), ct);

            await Send.OkAsync(new UpdateUserAvatarResponse(result.Success, result.RejectionReason), ct);
        }
    }
}
