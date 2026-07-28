using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using OCRWeb.Document.Application.Commands.CropPdf;

namespace OCRWeb.Document.Api.Endpoints.PdfFiles.CropPdfEndpoint;

/// <summary>
/// POST /api/pdf-files/{id}/crop — crop a PDF's content in place, replacing what was stored.
/// </summary>
public class CropPdfEndpoint(IMediator mediator) : Endpoint<CropPdfEndpointRequest, CropPdfEndpointResponse>
{
    public override void Configure()
    {
        Post("/api/pdf-files/{id}/crop");
        AuthSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CropPdfEndpointRequest req, CancellationToken ct)
    {
        var newId = await mediator.Send(
            new CropPdfCommand(req.Id, req.PageNo, req.CropX, req.CropY, req.Width, req.Height, req.FileName), ct);

        await Send.OkAsync(new CropPdfEndpointResponse(newId), ct);
    }
}
