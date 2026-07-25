using MediatR;
using OCRWeb.ProjectManagement.Domain.Repositories;
using OrangepuffPortal.Shared.Auditing;

namespace OCRWeb.ProjectManagement.Application.Commands.DeleteProject;

public class DeleteProjectCommandHandler(IProjectRepository repository, ICurrentUser currentUser)
    : IRequestHandler<DeleteProjectCommand>
{
    public async Task Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Project {request.Id} not found.");

        if (project.InsertedUserId != currentUser.UserId)
            throw new UnauthorizedAccessException("You do not own this project.");

        repository.Remove(project);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
