using MediatR;
using OCRWeb.Document.Domain.Repositories;
using OrangepuffPortal.Shared.Auditing;

namespace OCRWeb.Document.Application.Commands.DeletePdfFile;

public class DeletePdfFileCommandHandler(IPdfFileRepository repository, ICurrentUser currentUser)
    : IRequestHandler<DeletePdfFileCommand>
{
    public async Task Handle(DeletePdfFileCommand request, CancellationToken cancellationToken)
    {
        var file = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"PDF file {request.Id} not found.");

        if (file.InsertedUserId != currentUser.UserId)
            throw new UnauthorizedAccessException("You do not own this PDF file.");

        repository.Remove(file);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
