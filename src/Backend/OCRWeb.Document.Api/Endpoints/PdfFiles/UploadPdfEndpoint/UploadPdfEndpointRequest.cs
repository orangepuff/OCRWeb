using Microsoft.AspNetCore.Http;

namespace OCRWeb.Document.Api.Endpoints.PdfFiles.UploadPdfEndpoint
{
    public class UploadPdfEndpointRequest
    {
        public int ProjectId { get; set; }
        public IFormFile File { get; set; } = default!;
    }
}
