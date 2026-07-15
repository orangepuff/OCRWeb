using FastEndpoints;
using MediatR;
using OCRWeb.Contract.DocumentProcessing;
using OCRWeb.DocumentProcessing.Application.Queries.ListPdfFiles;

namespace OCRWeb.API.Endpoints.PdfFiles;

public class ListPdfFilesEndpointRequest
{
    public Guid ProjectId { get; set; }
}

/// <summary>GET /pdf-files?projectId= — list a project's PDF files (metadata only).</summary>
public class ListPdfFilesEndpoint(IMediator mediator)
    : Endpoint<ListPdfFilesEndpointRequest, IReadOnlyList<PdfFileListItemDto>>
{
    public override void Configure()
    {
        Get("/pdf-files");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ListPdfFilesEndpointRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new ListPdfFilesQuery(req.ProjectId), ct);
        await Send.OkAsync(result, ct);
    }
}
