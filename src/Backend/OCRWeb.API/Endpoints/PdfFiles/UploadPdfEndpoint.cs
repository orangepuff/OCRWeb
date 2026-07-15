using FastEndpoints;
using MediatR;
using OCRWeb.DocumentProcessing.Application.Commands.UploadPdf;

namespace OCRWeb.API.Endpoints.PdfFiles;

public class UploadPdfEndpointRequest
{
    public Guid ProjectId { get; set; }
    public IFormFile File { get; set; } = default!;
}

public record UploadPdfEndpointResponse(Guid Id);

/// <summary>POST /pdf-files — upload an original PDF (multipart form).</summary>
public class UploadPdfEndpoint(IMediator mediator)
    : Endpoint<UploadPdfEndpointRequest, UploadPdfEndpointResponse>
{
    public override void Configure()
    {
        Post("/pdf-files");
        AllowFileUploads();
        AllowAnonymous(); // no auth yet
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
