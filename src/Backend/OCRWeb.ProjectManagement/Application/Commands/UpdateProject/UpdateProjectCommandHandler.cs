using MediatR;
using OCRWeb.ProjectManagement.Contract;
using OCRWeb.ProjectManagement.Domain.Repositories;
using OrangepuffPortal.Shared.Auditing;

namespace OCRWeb.ProjectManagement.Application.Commands.UpdateProject;

public class UpdateProjectCommandHandler(IProjectRepository repository, ICurrentUser currentUser)
    : IRequestHandler<UpdateProjectCommand, ProjectListItemDto>
{
    public async Task<ProjectListItemDto> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Project {request.Id} not found.");

        if (project.InsertedUserId != currentUser.UserId)
            throw new UnauthorizedAccessException("You do not own this project.");

        project.Rename(request.Name, currentUser.UserId, DateTime.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);

        return new ProjectListItemDto(project.Id, project.Name, project.InsertedTime);
    }
}
