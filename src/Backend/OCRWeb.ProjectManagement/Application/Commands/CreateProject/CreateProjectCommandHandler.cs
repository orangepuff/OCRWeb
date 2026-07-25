using MediatR;
using OCRWeb.ProjectManagement.Domain.Entity;
using OCRWeb.ProjectManagement.Domain.Repositories;
using OrangepuffPortal.Shared.Auditing;

namespace OCRWeb.ProjectManagement.Application.Commands.CreateProject;

public class CreateProjectCommandHandler(IProjectRepository repository, ICurrentUser currentUser)
    : IRequestHandler<CreateProjectCommand, Guid>
{
    public async Task<Guid> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = Project.Create(request.Name, currentUser.UserId, DateTime.UtcNow);
        await repository.AddAsync(project, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return project.Id;
    }
}
