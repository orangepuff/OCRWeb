using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using OCRWeb.Document.Application.Commands.DeletePdfFile;

namespace OCRWeb.Document.Api.Endpoints.DeletePdfFile;

/// <summary>
/// DELETE /api/pdf-files/{Id} — delete a PDF file owned by the current user.
/// </summary>
public class DeletePdfFileEndpoint(IMediator mediator) : Endpoint<DeletePdfFileRequest, EmptyResponse>
{
    public override void Configure()
    {
        Delete("/api/pdf-files/{Id}");
        AuthSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(DeletePdfFileRequest req, CancellationToken ct)
    {
        try
        {
            await mediator.Send(new DeletePdfFileCommand(req.Id), ct);
            await Send.NoContentAsync(ct);
        }
        catch (KeyNotFoundException)
        {
            await Send.NotFoundAsync(ct);
        }
        catch (UnauthorizedAccessException)
        {
            await Send.ForbiddenAsync(ct);
        }
    }
}
