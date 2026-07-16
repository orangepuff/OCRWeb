using MediatR;
using OCRWeb.Document.Contract;
using OCRWeb.Document.Domain.Repositories;

namespace OCRWeb.Document.Application.Queries.ListPdfFiles;

public class ListPdfFilesQueryHandler(IPdfFileRepository repository)
    : IRequestHandler<ListPdfFilesQuery, IReadOnlyList<PdfFileListItemDto>>
{
    public async Task<IReadOnlyList<PdfFileListItemDto>> Handle(
        ListPdfFilesQuery request, CancellationToken cancellationToken)
    {
        var files = await repository.ListByProjectAsync(request.ProjectId, cancellationToken);
        return files
            .Select(f => new PdfFileListItemDto(
                f.Id, f.ProjectId, f.FileName, f.ContentType, f.SizeBytes, f.FileType.ToString(), f.InsertedTime))
            .ToList();
    }
}
