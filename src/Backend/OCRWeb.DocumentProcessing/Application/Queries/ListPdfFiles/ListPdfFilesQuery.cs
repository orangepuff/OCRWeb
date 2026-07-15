using MediatR;
using OCRWeb.Contract.DocumentProcessing;

namespace OCRWeb.DocumentProcessing.Application.Queries.ListPdfFiles;

/// <summary>List a project's PDF files (metadata only).</summary>
public record ListPdfFilesQuery(Guid ProjectId) : IRequest<IReadOnlyList<PdfFileListItemDto>>;
