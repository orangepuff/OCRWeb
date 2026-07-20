using MediatR;
using OCRWeb.Pdf.Contract;
using OCRWeb.Document.Domain.Entity;
using OCRWeb.Document.Domain.Enums;
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
        var name = string.IsNullOrWhiteSpace(request.FileName) ? $"cropped-{source.FileName}" : request.FileName!;

        var derived = PdfFile.CreateDerived(
            source.ProjectId, name, source.ContentType, croppedBytes,
            PdfFileType.Cropped, properties, currentUser.UserId, now);

        await repository.AddAsync(derived, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return derived.Id;
    }
}
