using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using OCRWeb.Document.Application.Queries.GetPdfFileContent;

namespace OCRWeb.Document.Api.Endpoints.PdfFiles;

public class GetPdfFileContentEndpointRequest
{
    public int Id { get; set; }
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

        await using var stream = content.Content;

        // A file's bytes never change once uploaded (upload/crop always mint a new id rather than
        // mutating an existing one), so the id alone is a valid ETag - no content hashing needed.
        // "private" keeps this out of any shared/CDN cache since the content is per-user cookie-
        // authenticated; max-age bounds how long a browser can serve it without re-checking auth
        // against the server at all.
        var etag = $"\"pdf-{req.Id}\"";
        HttpContext.Response.Headers.ETag = etag;
        HttpContext.Response.Headers.CacheControl = "private, max-age=3600";

        // Once max-age lapses, the browser revalidates with If-None-Match instead of assuming the
        // cached copy is stale - answering with 304 here skips re-transferring the (possibly large)
        // body while still hitting this auth-gated endpoint on every load past the cache window.
        if (HttpContext.Request.Headers.IfNoneMatch == etag)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status304NotModified;
            return;
        }

        // Send.BytesAsync always forces Content-Disposition: attachment when given a filename,
        // which pops the browser's Save dialog instead of rendering the PDF in a new tab. Set the
        // headers ourselves with "inline" so clicking the link opens the built-in PDF viewer.
        var contentDisposition = new ContentDispositionHeaderValue("inline");
        contentDisposition.SetHttpFileName(content.FileName);
        HttpContext.Response.Headers.ContentDisposition = contentDisposition.ToString();
        HttpContext.Response.ContentType = content.ContentType;

        // Set explicitly so the response isn't chunked - without it the browser has no
        // total size to report download progress against.
        HttpContext.Response.ContentLength = content.SizeBytes;

        // Streamed straight from the DB reader to the response body - bytes start flowing
        // to the client as soon as they're read off the wire, instead of only after the
        // entire blob has been buffered into memory.
        await stream.CopyToAsync(HttpContext.Response.Body, ct);
    }
}
