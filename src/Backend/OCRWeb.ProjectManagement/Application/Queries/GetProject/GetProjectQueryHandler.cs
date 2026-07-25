using MediatR;
using OCRWeb.ProjectManagement.Contract;
using OCRWeb.ProjectManagement.Domain.Repositories;
using OrangepuffPortal.Shared.Auditing;

namespace OCRWeb.ProjectManagement.Application.Queries.GetProject;

public class GetProjectQueryHandler(IProjectRepository repository, ICurrentUser currentUser)
    : IRequestHandler<GetProjectQuery, ProjectListItemDto>
{
    public async Task<ProjectListItemDto> Handle(GetProjectQuery request, CancellationToken cancellationToken)
    {
        var project = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Project {request.Id} not found.");

        if (project.InsertedUserId != currentUser.UserId)
            throw new UnauthorizedAccessException("You do not own this project.");

        return new ProjectListItemDto(project.Id, project.Name, project.InsertedTime);
    }
}
