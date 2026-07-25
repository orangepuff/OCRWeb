using MediatR;
using OCRWeb.ProjectManagement.Contract;
using OCRWeb.ProjectManagement.Domain.Repositories;
using OrangepuffPortal.Shared.Auditing;

namespace OCRWeb.ProjectManagement.Application.Queries.ListProjects;

public class ListProjectsQueryHandler(IProjectRepository repository, ICurrentUser currentUser)
    : IRequestHandler<ListProjectsQuery, IReadOnlyList<ProjectListItemDto>>
{
    public async Task<IReadOnlyList<ProjectListItemDto>> Handle(
        ListProjectsQuery request, CancellationToken cancellationToken)
    {
        var projects = await repository.ListByOwnerAsync(currentUser.UserId, cancellationToken);
        return projects
            .Select(p => new ProjectListItemDto(p.Id, p.Name, p.InsertedTime))
            .ToList();
    }
}
