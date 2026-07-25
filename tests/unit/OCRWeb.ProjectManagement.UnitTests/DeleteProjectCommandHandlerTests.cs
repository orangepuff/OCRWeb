using MediatR;
using Moq;
using OCRWeb.ProjectManagement.Application.Commands.DeleteProject;
using OCRWeb.ProjectManagement.Contract;
using OCRWeb.ProjectManagement.Domain.Entity;
using OCRWeb.ProjectManagement.Domain.Repositories;
using OrangepuffPortal.Shared.Auditing;

namespace OCRWeb.ProjectManagement.UnitTests;

public class DeleteProjectCommandHandlerTests
{
    [Fact]
    public async Task Handle_removes_owned_project()
    {
        var project = Project.Create("Mine", userId: 7, DateTime.UtcNow);

        var repo = new Mock<IProjectRepository>();
        repo.Setup(r => r.GetByIdAsync(project.Id, It.IsAny<CancellationToken>())).ReturnsAsync(project);

        var currentUser = new Mock<ICurrentUser>();
        currentUser.SetupGet(u => u.UserId).Returns(7);

        var publisher = new Mock<IPublisher>();

        var handler = new DeleteProjectCommandHandler(repo.Object, currentUser.Object, publisher.Object);
        await handler.Handle(new DeleteProjectCommand(project.Id), CancellationToken.None);

        repo.Verify(r => r.Remove(project), Times.Once);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        publisher.Verify(
            p => p.Publish(It.Is<ProjectDeletedNotification>(n => n.ProjectId == project.Id), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_missing_project_throws_KeyNotFoundException()
    {
        var repo = new Mock<IProjectRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Project?)null);

        var handler = new DeleteProjectCommandHandler(repo.Object, Mock.Of<ICurrentUser>(), Mock.Of<IPublisher>());

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new DeleteProjectCommand(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_project_owned_by_another_user_throws_UnauthorizedAccessException()
    {
        var project = Project.Create("Someone Else's", userId: 1, DateTime.UtcNow);

        var repo = new Mock<IProjectRepository>();
        repo.Setup(r => r.GetByIdAsync(project.Id, It.IsAny<CancellationToken>())).ReturnsAsync(project);

        var currentUser = new Mock<ICurrentUser>();
        currentUser.SetupGet(u => u.UserId).Returns(2);

        var handler = new DeleteProjectCommandHandler(repo.Object, currentUser.Object, Mock.Of<IPublisher>());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new DeleteProjectCommand(project.Id), CancellationToken.None));

        repo.Verify(r => r.Remove(It.IsAny<Project>()), Times.Never);
    }
}
