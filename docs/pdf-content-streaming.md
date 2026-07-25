# PDF content streaming

`GET /api/pdf-files/{id}/content` (`GetPdfFileContentEndpoint`) serves a PDF's binary content
for preview/download. For large files (100s of MB), the original implementation loaded the
entire blob into memory via EF Core (`PdfFileRepository.GetWithContentAsync`, an `Include(x =>
x.Content)`) before writing any bytes to the HTTP response. The client saw zero response bytes,
and therefore zero download progress, for the whole DB-read+materialize duration — indistinguishable
from a hung request.

## Design

`GetPdfFileContentQueryHandler` now composes two repository calls instead of one:

1. `IPdfFileRepository.GetByIdAsync` — cheap EF Core query for metadata only (`FileName`,
   `ContentType`, `SizeBytes`), used for response headers.
2. `IPdfFileRepository.OpenContentStreamAsync` — opens a `Stream` over the `binContent` column
   without materializing it, by bypassing EF Core:
   - Raw `Microsoft.Data.SqlClient` `SqlCommand` against `[docproc].[PDFFileContents]`.
   - `CommandBehavior.SequentialAccess | CommandBehavior.SingleRow` + `SqlDataReader.GetStream(0)`,
     so bytes are read off the wire on demand instead of buffered up front.
   - The returned stream (`SqlBlobStream`) wraps the `SqlConnection`/`SqlCommand`/`SqlDataReader`
     so all four objects are disposed together when the caller disposes the stream — otherwise a
     bare `GetStream()` result gives no way to close the reader/connection once done.

`GetPdfFileContentEndpoint` sets `Content-Length` from the metadata's `SizeBytes` (so the browser
can compute download progress) and `CopyToAsync`s the stream directly to `HttpContext.Response.Body`.

`PdfFileContentDto.Content` changed from `byte[]` to `Stream`; the caller (the endpoint) is
responsible for disposing it (`await using`).

## Scope

`CropPdfCommandHandler` is unaffected — cropping needs the full byte array in memory anyway
(PDFsharp has no streaming API), so it still uses `GetWithContentAsync`.

`Microsoft.Data.SqlClient` is now referenced directly by `OCRWeb.Document` (previously only a
transitive dependency via `Microsoft.EntityFrameworkCore.SqlServer`); the central package pin in
`Directory.Packages.props` was bumped from `6.0.2` to `6.1.1` to match what EF Core's SqlServer
provider already required transitively (the old pin was a downgrade, which NuGet treats as an
error under this repo's warn-as-error settings).
