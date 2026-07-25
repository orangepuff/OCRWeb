using MediatR;
using OCRWeb.Document.Contract;
using OCRWeb.Document.Domain.Repositories;

namespace OCRWeb.Document.Application.Queries.GetPdfFileContent;

public class GetPdfFileContentQueryHandler(IPdfFileRepository repository)
    : IRequestHandler<GetPdfFileContentQuery, PdfFileContentDto?>
{
    public async Task<PdfFileContentDto?> Handle(GetPdfFileContentQuery request, CancellationToken cancellationToken)
    {
        var file = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (file is null)
        {
            return null;
        }

        var stream = await repository.OpenContentStreamAsync(request.Id, cancellationToken);
        if (stream is null)
        {
            return null;
        }

        return new PdfFileContentDto(stream, file.ContentType, file.FileName, file.SizeBytes);
    }
}
