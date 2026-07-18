using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OCRWeb.Document.Contract;
using OCRWeb.Document.Application.Queries.ListPdfFiles;

namespace OCRWeb.Document.Api.Endpoints.PdfFiles;

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
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(ListPdfFilesEndpointRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new ListPdfFilesQuery(req.ProjectId), ct);
        await Send.OkAsync(result, ct);
    }
}
