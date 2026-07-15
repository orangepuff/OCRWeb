using FastEndpoints;
using MediatR;
using OCRWeb.DocumentProcessing.Application.Commands.CropPdf;

namespace OCRWeb.API.Endpoints.PdfFiles;

public class CropPdfEndpointRequest
{
    public Guid Id { get; set; }         // source PDF file id (route)
    public int PageNo { get; set; }
    public int CropX { get; set; }
    public int CropY { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string? FileName { get; set; }
}

public record CropPdfEndpointResponse(Guid Id);

/// <summary>POST /pdf-files/{id}/crop — crop a source PDF into a new derived file.</summary>
public class CropPdfEndpoint(IMediator mediator)
    : Endpoint<CropPdfEndpointRequest, CropPdfEndpointResponse>
{
    public override void Configure()
    {
        Post("/pdf-files/{id}/crop");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CropPdfEndpointRequest req, CancellationToken ct)
    {
        var newId = await mediator.Send(
            new CropPdfCommand(req.Id, req.PageNo, req.CropX, req.CropY, req.Width, req.Height, req.FileName), ct);

        await Send.OkAsync(new CropPdfEndpointResponse(newId), ct);
    }
}
