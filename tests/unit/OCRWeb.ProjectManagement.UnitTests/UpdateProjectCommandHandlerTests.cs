using Moq;
using OCRWeb.ProjectManagement.Application.Commands.UpdateProject;
using OCRWeb.ProjectManagement.Domain.Entity;
using OCRWeb.ProjectManagement.Domain.Repositories;
using OrangepuffPortal.Shared.Auditing;

namespace OCRWeb.ProjectManagement.UnitTests;

public class UpdateProjectCommandHandlerTests
{
    [Fact]
    public async Task Handle_renames_owned_project()
    {
        var project = Project.Create("Old Name", userId: 7, DateTime.UtcNow);

        var repo = new Mock<IProjectRepository>();
        repo.Setup(r => r.GetByIdAsync(project.Id, It.IsAny<CancellationToken>())).ReturnsAsync(project);

        var currentUser = new Mock<ICurrentUser>();
        currentUser.SetupGet(u => u.UserId).Returns(7);

        var handler = new UpdateProjectCommandHandler(repo.Object, currentUser.Object);
        var dto = await handler.Handle(new UpdateProjectCommand(project.Id, "New Name"), CancellationToken.None);

        Assert.Equal("New Name", dto.Name);
        Assert.Equal("New Name", project.Name);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_missing_project_throws_KeyNotFoundException()
    {
        var repo = new Mock<IProjectRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Project?)null);

        var handler = new UpdateProjectCommandHandler(repo.Object, Mock.Of<ICurrentUser>());

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new UpdateProjectCommand(Guid.NewGuid(), "New Name"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_project_owned_by_another_user_throws_UnauthorizedAccessException()
    {
        var project = Project.Create("Someone Else's", userId: 1, DateTime.UtcNow);

        var repo = new Mock<IProjectRepository>();
        repo.Setup(r => r.GetByIdAsync(project.Id, It.IsAny<CancellationToken>())).ReturnsAsync(project);

        var currentUser = new Mock<ICurrentUser>();
        currentUser.SetupGet(u => u.UserId).Returns(2);

        var handler = new UpdateProjectCommandHandler(repo.Object, currentUser.Object);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new UpdateProjectCommand(project.Id, "New Name"), CancellationToken.None));
    }
}
