using MediatR;
using Microsoft.Extensions.Logging;
using OCRWeb.Pdf.Contract;
using OCRWeb.Document.Domain.Repositories;
using OCRWeb.Document.Domain.ValueObjects;
using OrangepuffPortal.Shared.Auditing;

namespace OCRWeb.Document.Application.Commands.CropPdf;

public class CropPdfCommandHandler(
    IPdfFileRepository repository,
    IPdfManipulator manipulator,
    ICurrentUser currentUser,
    ILogger<CropPdfCommandHandler> logger) : IRequestHandler<CropPdfCommand, int>
{
    private const string LogPrefix = nameof(CropPdfCommandHandler) + "." + nameof(Handle);

    public async Task<int> Handle(CropPdfCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("{LogPrefix}: cropping file {FileId} page {PageNo} for user {UserId}", LogPrefix, request.SourcePdfFileId, request.PageNo, currentUser.UserId);

        try
        {
            var source = await repository.GetWithContentAsync(request.SourcePdfFileId, cancellationToken);

            if(source == null)
            {
                logger.LogError("{LogPrefix}: PDF file { request.SourcePdfFileId} was not found.", LogPrefix, request.SourcePdfFileId);
                throw new KeyNotFoundException($"PDF file {request.SourcePdfFileId} was not found.");
            }

            var area = new PdfCropArea(request.PageNo, request.CropX, request.CropY, request.Width, request.Height);
            var croppedBytes = manipulator.Crop(source.Content.Content, area);

            var now = DateTime.UtcNow;
            var properties = new FileProperties(request.PageNo, request.CropX, request.CropY, request.Width, request.Height);
            var name = string.IsNullOrWhiteSpace(request.FileName) ? source.FileName : request.FileName!;

            // Crop replaces the source file's own content rather than creating a separate derived
            // file - there's only ever one current file per project, and cropping just updates it.
            source.ApplyCrop(croppedBytes, name, properties, currentUser.UserId, now);
            await repository.SaveChangesAsync(cancellationToken);

            logger.LogInformation("{LogPrefix}: cropped file {FileId} → {Bytes} bytes", LogPrefix, source.Id, croppedBytes.Length);
            return source.Id;
        }
        catch(Exception ex)
        {
            logger.LogError("{LogPrefix}: Exception: {Exception}", LogPrefix, ex.Message);
            throw;
        }
    }
}
