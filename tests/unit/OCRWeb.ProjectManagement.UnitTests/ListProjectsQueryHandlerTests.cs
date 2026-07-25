using Moq;
using OCRWeb.ProjectManagement.Application.Queries.ListProjects;
using OCRWeb.ProjectManagement.Domain.Entity;
using OCRWeb.ProjectManagement.Domain.Repositories;
using OrangepuffPortal.Shared.Auditing;

namespace OCRWeb.ProjectManagement.UnitTests;

public class ListProjectsQueryHandlerTests
{
    [Fact]
    public async Task Handle_returns_only_current_users_projects_mapped_to_dto()
    {
        var owned = Project.Create("Mine", userId: 7, DateTime.UtcNow);

        var repo = new Mock<IProjectRepository>();
        repo.Setup(r => r.ListByOwnerAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync([owned]);

        var currentUser = new Mock<ICurrentUser>();
        currentUser.SetupGet(u => u.UserId).Returns(7);

        var handler = new ListProjectsQueryHandler(repo.Object, currentUser.Object);
        var result = await handler.Handle(new ListProjectsQuery(), CancellationToken.None);

        var item = Assert.Single(result);
        Assert.Equal(owned.Id, item.Id);
        Assert.Equal("Mine", item.Name);
        repo.Verify(r => r.ListByOwnerAsync(7, It.IsAny<CancellationToken>()), Times.Once);
    }
}
