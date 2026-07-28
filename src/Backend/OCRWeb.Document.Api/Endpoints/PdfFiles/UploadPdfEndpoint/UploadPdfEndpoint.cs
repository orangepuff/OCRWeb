using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using OCRWeb.Document.Application.Commands.UploadPdf;
using OCRWeb.Document.Domain.Exceptions;

namespace OCRWeb.Document.Api.Endpoints.PdfFiles.UploadPdfEndpoint;

/// <summary>
/// POST /api/pdf-files — upload an original PDF (multipart form).
/// </summary>
public class UploadPdfEndpoint(IMediator mediator) : Endpoint<UploadPdfEndpointRequest, UploadPdfEndpointResponse>
{
    public override void Configure()
    {
        Post("/api/pdf-files");
        AllowFileUploads();
        AuthSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UploadPdfEndpointRequest req, CancellationToken ct)
    {
        using var ms = new MemoryStream();
        await req.File.CopyToAsync(ms, ct);

        try
        {
            var id = await mediator.Send(
                new UploadPdfCommand(
                    req.ProjectId,
                    req.File.FileName,
                    string.IsNullOrWhiteSpace(req.File.ContentType) ? "application/pdf" : req.File.ContentType,
                    ms.ToArray()),
                ct);

            await Send.OkAsync(new UploadPdfEndpointResponse(id), ct);
        }
        catch (DomainValidationException ex)
        {
            ThrowError(r => r.File, ex.Message);
        }
    }
}
