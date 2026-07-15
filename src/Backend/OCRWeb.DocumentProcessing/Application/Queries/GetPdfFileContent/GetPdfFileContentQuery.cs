using MediatR;
using OCRWeb.Contract.DocumentProcessing;

namespace OCRWeb.DocumentProcessing.Application.Queries.GetPdfFileContent;

/// <summary>Fetch a PDF's binary content for download/streaming.</summary>
public record GetPdfFileContentQuery(Guid Id) : IRequest<PdfFileContentDto?>;
