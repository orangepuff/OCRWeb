using MediatR;
using OCRWeb.Document.Contract;
using OCRWeb.Document.Domain.Repositories;

namespace OCRWeb.Document.Application.Queries.GetPdfFileDetail;

public class GetPdfFileDetailQueryHandler(IPdfFileRepository repository)
    : IRequestHandler<GetPdfFileDetailQuery, PdfFileDetailDto?>
{
    public async Task<PdfFileDetailDto?> Handle(GetPdfFileDetailQuery request, CancellationToken cancellationToken)
    {
        var f = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (f is null)
            return null;

        var props = f.Properties is null
            ? null
            : new FilePropertiesDto(f.Properties.PageNo, f.Properties.CropX, f.Properties.CropY, f.Properties.Width, f.Properties.Height);

        return new PdfFileDetailDto(
            f.Id, f.ProjectId, f.FileName, f.ContentType, f.SizeBytes, f.FileType.ToString(), props,
            f.InsertedUserId, f.InsertedTime, f.UpdatedUserId, f.UpdatedTime);
    }
}
