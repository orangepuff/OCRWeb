using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using OCRWeb.Document.Application.Commands.UploadPdf;

namespace OCRWeb.Document.Api.Endpoints.PdfFiles;

public class UploadPdfEndpointRequest
{
    public int ProjectId { get; set; }
    public IFormFile File { get; set; } = default!;
}

public record UploadPdfEndpointResponse(int Id);

/// <summary>POST /api/pdf-files — upload an original PDF (multipart form).</summary>
public class UploadPdfEndpoint(IMediator mediator)
    : Endpoint<UploadPdfEndpointRequest, UploadPdfEndpointResponse>
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

        var id = await mediator.Send(
            new UploadPdfCommand(
                req.ProjectId,
                req.File.FileName,
                string.IsNullOrWhiteSpace(req.File.ContentType) ? "application/pdf" : req.File.ContentType,
                ms.ToArray()),
            ct);

        await Send.OkAsync(new UploadPdfEndpointResponse(id), ct);
    }
}
