using MediatR;
using OCRWeb.Document.Contract;

namespace OCRWeb.Document.Application.Queries.GetPdfFileDetail;

/// <summary>Full metadata for one PDF file (no binary content).</summary>
public record GetPdfFileDetailQuery(Guid Id) : IRequest<PdfFileDetailDto?>;
