using MediatR;
using OCRWeb.Document.Contract;

namespace OCRWeb.Document.Application.Queries.ListPdfFiles;

/// <summary>List a project's PDF files (metadata only).</summary>
public record ListPdfFilesQuery(Guid ProjectId) : IRequest<IReadOnlyList<PdfFileListItemDto>>;
