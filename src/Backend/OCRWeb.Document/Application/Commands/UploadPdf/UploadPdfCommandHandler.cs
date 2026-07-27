using MediatR;
using OCRWeb.Document.Domain.Entity;
using OCRWeb.Document.Domain.Repositories;
using OrangepuffPortal.Config.Contract.Interfaces;
using OrangepuffPortal.Shared.Auditing;

namespace OCRWeb.Document.Application.Commands.UploadPdf;

public class UploadPdfCommandHandler(
    IPdfFileRepository repository,
    ICurrentUser currentUser,
    ICurrentUserConfig currentUserConfig)
    : IRequestHandler<UploadPdfCommand, int>
{
    private const string MaxUploadSizeConfigCode = "OCRWeb.MaxUploadSizeBytes";
    private const int DefaultMaxUploadSizeBytes = 104_857_600; // 100 MB — matches the seeded default in Program.cs

    public async Task<int> Handle(UploadPdfCommand request, CancellationToken cancellationToken)
    {
        var maxUploadSizeBytes = currentUserConfig.GetInt(MaxUploadSizeConfigCode, DefaultMaxUploadSizeBytes);
        if (request.Content.Length > maxUploadSizeBytes)
        {
            throw new InvalidOperationException(
                $"Upload of {request.Content.Length} byte(s) exceeds this user's {maxUploadSizeBytes} byte limit.");
        }

        var now = DateTime.UtcNow;
        var file = PdfFile.CreateOriginal(
            request.ProjectId, request.FileName, request.ContentType, request.Content, currentUser.UserId, now);

        await repository.AddAsync(file, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return file.Id;
    }
}
