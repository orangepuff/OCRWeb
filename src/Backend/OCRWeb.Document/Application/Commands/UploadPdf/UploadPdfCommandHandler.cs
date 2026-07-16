using MediatR;
using OCRWeb.Document.Domain.Entity;
using OCRWeb.Document.Domain.Repositories;
using OCRWeb.Shared.Auditing;

namespace OCRWeb.Document.Application.Commands.UploadPdf;

public class UploadPdfCommandHandler(IPdfFileRepository repository, ICurrentUser currentUser)
    : IRequestHandler<UploadPdfCommand, Guid>
{
    public async Task<Guid> Handle(UploadPdfCommand request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var file = PdfFile.CreateOriginal(
            request.ProjectId, request.FileName, request.ContentType, request.Content, currentUser.UserId, now);

        await repository.AddAsync(file, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return file.Id;
    }
}
