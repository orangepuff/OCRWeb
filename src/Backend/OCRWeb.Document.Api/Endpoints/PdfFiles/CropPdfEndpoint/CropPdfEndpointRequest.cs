namespace OCRWeb.Document.Api.Endpoints.PdfFiles.CropPdfEndpoint
{
    public class CropPdfEndpointRequest
    {
        public int Id { get; set; }          // source PDF file id (route)
        public int PageNo { get; set; }
        public int CropX { get; set; }
        public int CropY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string? FileName { get; set; }
    }
}
