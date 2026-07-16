using FastEndpoints;
using MediatR;
using OCRWeb.Document.Application.Queries.GetPdfFileContent;

namespace OCRWeb.Document.Api.Endpoints.PdfFiles;

public class GetPdfFileContentEndpointRequest
{
    public Guid Id { get; set; }
}

/// <summary>GET /pdf-files/{id}/content — stream a PDF's binary content.</summary>
public class GetPdfFileContentEndpoint(IMediator mediator)
    : Endpoint<GetPdfFileContentEndpointRequest>
{
    public override void Configure()
    {
        Get("/pdf-files/{id}/content");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetPdfFileContentEndpointRequest req, CancellationToken ct)
    {
        var content = await mediator.Send(new GetPdfFileContentQuery(req.Id), ct);
        if (content is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.BytesAsync(content.Content, content.FileName, content.ContentType, cancellation: ct);
    }
}
