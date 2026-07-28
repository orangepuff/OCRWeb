using MediatR;
using Microsoft.Extensions.Logging;
using OCRWeb.Shared.Const;
using OCRWeb.Document.Domain.Entity;
using OCRWeb.Document.Domain.Exceptions;
using OCRWeb.Document.Domain.Repositories;
using OrangepuffPortal.Config.Contract.Interfaces;
using OrangepuffPortal.Shared.Auditing;

namespace OCRWeb.Document.Application.Commands.UploadPdf;

public class UploadPdfCommandHandler(
    IPdfFileRepository repository,
    ICurrentUser currentUser,
    ICurrentUserConfig currentUserConfig,
    ILogger<UploadPdfCommandHandler> logger) : IRequestHandler<UploadPdfCommand, int>
{
    private const int DefaultMaxUploadSizeBytes = 104_857_600; // 100 MB
    private const string LogPrefix = nameof(UploadPdfCommandHandler) + "." + nameof(Handle);

    public async Task<int> Handle(UploadPdfCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("{LogPrefix}: Start uploading file to ProjectId : {ProjectId}, FileName: {FileName}, FileLength: {FileLength} by UserId: {UserId}", LogPrefix, request.ProjectId, request.FileName, request.Content.Length, currentUser.UserId);

        var maxBytes = currentUserConfig.GetInt(ConfigKeys.MaximumFileUploadSize, DefaultMaxUploadSizeBytes);
        if (request.Content.Length > maxBytes)
        {
            logger.LogError("{LogPrefix}: Upload of {FileLength} byte(s) exceeds the {ConfigLimit}", LogPrefix, request.Content.Length, maxBytes);

            throw new DomainValidationException($"Upload of {request.Content.Length} byte(s) exceeds the {maxBytes} byte limit.");
        }

        var now = DateTime.UtcNow;
        var file = PdfFile.CreateOriginal(request.ProjectId, request.FileName, request.ContentType, request.Content, currentUser.UserId, now);

        await repository.AddAsync(file, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("{LogPrefix}: created PdfFile {FileId} ({FileName}, {Bytes} bytes) for project {ProjectId}",LogPrefix, file.Id, request.FileName, request.Content.Length, request.ProjectId);

        return file.Id;
    }
}
