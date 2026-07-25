using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Net.Http.Headers;
using OCRWeb.Document.Application.Queries.GetPdfFileContent;

namespace OCRWeb.Document.Api.Endpoints.PdfFiles;

public class GetPdfFileContentEndpointRequest
{
    public Guid Id { get; set; }
}

/// <summary>GET /api/pdf-files/{id}/content — stream a PDF's binary content.</summary>
public class GetPdfFileContentEndpoint(IMediator mediator)
    : Endpoint<GetPdfFileContentEndpointRequest>
{
    public override void Configure()
    {
        Get("/api/pdf-files/{id}/content");
        AuthSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetPdfFileContentEndpointRequest req, CancellationToken ct)
    {
        var content = await mediator.Send(new GetPdfFileContentQuery(req.Id), ct);
        if (content is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        // Send.BytesAsync always forces Content-Disposition: attachment when given a filename,
        // which pops the browser's Save dialog instead of rendering the PDF in a new tab. Set the
        // headers ourselves with "inline" so clicking the link opens the built-in PDF viewer.
        var contentDisposition = new ContentDispositionHeaderValue("inline");
        contentDisposition.SetHttpFileName(content.FileName);
        HttpContext.Response.Headers.ContentDisposition = contentDisposition.ToString();
        HttpContext.Response.ContentType = content.ContentType;
        await HttpContext.Response.Body.WriteAsync(content.Content, ct);
    }
}
