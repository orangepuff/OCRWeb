using FastEndpoints;
using MediatR;
using OCRWeb.Contract.DocumentProcessing;
using OCRWeb.DocumentProcessing.Application.Queries.GetPdfFileDetail;

namespace OCRWeb.API.Endpoints.PdfFiles;

public class GetPdfFileDetailEndpointRequest
{
    public Guid Id { get; set; }
}

/// <summary>GET /pdf-files/{id} — full metadata for one PDF file.</summary>
public class GetPdfFileDetailEndpoint(IMediator mediator)
    : Endpoint<GetPdfFileDetailEndpointRequest, PdfFileDetailDto>
{
    public override void Configure()
    {
        Get("/pdf-files/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetPdfFileDetailEndpointRequest req, CancellationToken ct)
    {
        var dto = await mediator.Send(new GetPdfFileDetailQuery(req.Id), ct);
        if (dto is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(dto, ct);
    }
}
