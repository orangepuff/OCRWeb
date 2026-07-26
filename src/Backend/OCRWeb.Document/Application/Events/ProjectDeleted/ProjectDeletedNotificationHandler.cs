using MediatR;
using OCRWeb.Document.Domain.Repositories;
using OCRWeb.ProjectManagement.Contract;

namespace OCRWeb.Document.Application.Events.ProjectDeleted;

/// <summary>
/// Hard-deletes every file that belonged to a deleted project; content cascades via FK.
/// </summary>
public class ProjectDeletedNotificationHandler(IPdfFileRepository repository) : INotificationHandler<ProjectDeletedNotification>
{
    public Task Handle(ProjectDeletedNotification notification, CancellationToken cancellationToken) => repository.RemoveAllByParentIdAsync(notification.ProjectId, cancellationToken);
}
