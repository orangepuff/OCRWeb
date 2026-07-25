using MediatR;
using OCRWeb.Pdf.Contract;
using OCRWeb.Document.Domain.Repositories;
using OCRWeb.Document.Domain.ValueObjects;
using OrangepuffPortal.Shared.Auditing;

namespace OCRWeb.Document.Application.Commands.CropPdf;

public class CropPdfCommandHandler(
    IPdfFileRepository repository,
    IPdfManipulator manipulator,
    ICurrentUser currentUser) : IRequestHandler<CropPdfCommand, Guid>
{
    public async Task<Guid> Handle(CropPdfCommand request, CancellationToken cancellationToken)
    {
        var source = await repository.GetWithContentAsync(request.SourcePdfFileId, cancellationToken)
            ?? throw new KeyNotFoundException($"PDF file {request.SourcePdfFileId} was not found.");

        var area = new PdfCropArea(request.PageNo, request.CropX, request.CropY, request.Width, request.Height);
        var croppedBytes = manipulator.Crop(source.Content.Content, area);

        var now = DateTime.UtcNow;
        var properties = new FileProperties(request.PageNo, request.CropX, request.CropY, request.Width, request.Height);
        var name = string.IsNullOrWhiteSpace(request.FileName) ? source.FileName : request.FileName!;

        // Crop replaces the source file's own content rather than creating a separate derived
        // file - there's only ever one current file per project, and cropping just updates it.
        source.ApplyCrop(croppedBytes, name, properties, currentUser.UserId, now);
        await repository.SaveChangesAsync(cancellationToken);

        return source.Id;
    }
}
