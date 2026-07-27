using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using OCRWeb.Document.Application.Commands.UploadPdf;
using OrangepuffPortal.Config.Contract.Interfaces;

namespace OCRWeb.Document.Api.Endpoints.PdfFiles;

public class UploadPdfEndpointRequest
{
    public int ProjectId { get; set; }
    public IFormFile File { get; set; } = default!;
}

public record UploadPdfEndpointResponse(int Id);

/// <summary>POST /api/pdf-files — upload an original PDF (multipart form).</summary>
public class UploadPdfEndpoint(IMediator mediator, ICurrentUserConfig currentUserConfig)
    : Endpoint<UploadPdfEndpointRequest, UploadPdfEndpointResponse>
{
    private const string MaxUploadSizeConfigCode = "OCRWeb.MaxUploadSizeBytes";
    private const int DefaultMaxUploadSizeBytes = 104_857_600; // 100 MB

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

        var maxUploadSizeBytes = currentUserConfig.GetInt(MaxUploadSizeConfigCode, DefaultMaxUploadSizeBytes);
        if (ms.Length > maxUploadSizeBytes)
        {
            ThrowError(r => r.File, $"Upload of {ms.Length} byte(s) exceeds the {maxUploadSizeBytes} byte limit.");
        }

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
