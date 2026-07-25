using Moq;
using OCRWeb.ProjectManagement.Application.Queries.GetProject;
using OCRWeb.ProjectManagement.Domain.Entity;
using OCRWeb.ProjectManagement.Domain.Repositories;
using OrangepuffPortal.Shared.Auditing;

namespace OCRWeb.ProjectManagement.UnitTests;

public class GetProjectQueryHandlerTests
{
    [Fact]
    public async Task Handle_returns_owned_project_as_dto()
    {
        var project = Project.Create("Mine", userId: 7, DateTime.UtcNow);

        var repo = new Mock<IProjectRepository>();
        repo.Setup(r => r.GetByIdAsync(project.Id, It.IsAny<CancellationToken>())).ReturnsAsync(project);

        var currentUser = new Mock<ICurrentUser>();
        currentUser.SetupGet(u => u.UserId).Returns(7);

        var handler = new GetProjectQueryHandler(repo.Object, currentUser.Object);
        var dto = await handler.Handle(new GetProjectQuery(project.Id), CancellationToken.None);

        Assert.Equal(project.Id, dto.Id);
        Assert.Equal("Mine", dto.Name);
    }

    [Fact]
    public async Task Handle_missing_project_throws_KeyNotFoundException()
    {
        var repo = new Mock<IProjectRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Project?)null);

        var handler = new GetProjectQueryHandler(repo.Object, Mock.Of<ICurrentUser>());

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new GetProjectQuery(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_project_owned_by_another_user_throws_UnauthorizedAccessException()
    {
        var project = Project.Create("Someone Else's", userId: 1, DateTime.UtcNow);

        var repo = new Mock<IProjectRepository>();
        repo.Setup(r => r.GetByIdAsync(project.Id, It.IsAny<CancellationToken>())).ReturnsAsync(project);

        var currentUser = new Mock<ICurrentUser>();
        currentUser.SetupGet(u => u.UserId).Returns(2);

        var handler = new GetProjectQueryHandler(repo.Object, currentUser.Object);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new GetProjectQuery(project.Id), CancellationToken.None));
    }
}
