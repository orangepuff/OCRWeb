using MediatR;
using OCRWeb.Contract.DocumentProcessing;

namespace OCRWeb.DocumentProcessing.Application.Queries.GetPdfFileDetail;

/// <summary>Full metadata for one PDF file (no binary content).</summary>
public record GetPdfFileDetailQuery(Guid Id) : IRequest<PdfFileDetailDto?>;
