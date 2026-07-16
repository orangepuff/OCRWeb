using MediatR;
using OCRWeb.Document.Contract;

namespace OCRWeb.Document.Application.Queries.GetPdfFileContent;

/// <summary>Fetch a PDF's binary content for download/streaming.</summary>
public record GetPdfFileContentQuery(Guid Id) : IRequest<PdfFileContentDto?>;
