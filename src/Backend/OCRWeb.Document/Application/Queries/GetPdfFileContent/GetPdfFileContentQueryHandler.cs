using MediatR;
using OCRWeb.Document.Contract;
using OCRWeb.Document.Domain.Repositories;

namespace OCRWeb.Document.Application.Queries.GetPdfFileContent;

public class GetPdfFileContentQueryHandler(IPdfFileRepository repository)
    : IRequestHandler<GetPdfFileContentQuery, PdfFileContentDto?>
{
    public async Task<PdfFileContentDto?> Handle(GetPdfFileContentQuery request, CancellationToken cancellationToken)
    {
        var file = await repository.GetWithContentAsync(request.Id, cancellationToken);
        return file is null
            ? null
            : new PdfFileContentDto(file.Content.Content, file.ContentType, file.FileName);
    }
}
