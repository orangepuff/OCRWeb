using Moq;
using OCRWeb.Document.Application.Events.ProjectDeleted;
using OCRWeb.Document.Domain.Repositories;
using OCRWeb.ProjectManagement.Contract;

namespace OCRWeb.Document.UnitTests;

public class ProjectDeletedNotificationHandlerTests
{
    [Fact]
    public async Task Handle_removes_all_files_for_the_deleted_project()
    {
        var projectId = Guid.NewGuid();
        var repo = new Mock<IPdfFileRepository>();
        repo.Setup(r => r.RemoveAllByParentIdAsync(projectId, It.IsAny<CancellationToken>())).ReturnsAsync(2);

        var handler = new ProjectDeletedNotificationHandler(repo.Object);
        await handler.Handle(new ProjectDeletedNotification(projectId), CancellationToken.None);

        repo.Verify(r => r.RemoveAllByParentIdAsync(projectId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
