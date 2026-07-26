using Moq;
using OCRWeb.ProjectManagement.Application.Commands.CreateProject;
using OCRWeb.ProjectManagement.Domain.Entity;
using OCRWeb.ProjectManagement.Domain.Repositories;
using OrangepuffPortal.Shared.Auditing;

namespace OCRWeb.ProjectManagement.UnitTests;

public class CreateProjectCommandHandlerTests
{
    [Fact]
    public async Task Handle_creates_project_owned_by_current_user()
    {
        Project? added = null;
        var repo = new Mock<IProjectRepository>();
        repo.Setup(r => r.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()))
            .Callback<Project, CancellationToken>((p, _) => added = p)
            .Returns(Task.CompletedTask);

        var currentUser = new Mock<ICurrentUser>();
        currentUser.SetupGet(u => u.UserId).Returns(7);

        var handler = new CreateProjectCommandHandler(repo.Object, currentUser.Object);
        var id = await handler.Handle(new CreateProjectCommand("My Project"), CancellationToken.None);

        // Id is DB-generated (identity) and only populated by a real SaveChanges, so a mocked
        // repository leaves it at the default - what matters here is the handler returns
        // whatever ends up on the entity it persisted, not a specific non-zero value.
        Assert.NotNull(added);
        Assert.Equal(added.Id, id);
        Assert.Equal("My Project", added.Name);
        Assert.Equal(7, added.InsertedUserId);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_blank_name_throws()
    {
        var repo = new Mock<IProjectRepository>();
        var handler = new CreateProjectCommandHandler(repo.Object, Mock.Of<ICurrentUser>());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.Handle(new CreateProjectCommand("   "), CancellationToken.None));
    }
}
