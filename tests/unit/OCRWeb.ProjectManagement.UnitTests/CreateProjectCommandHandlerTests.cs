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
        var repo = new Mock<IProjectRepository>();

        var currentUser = new Mock<ICurrentUser>();
        currentUser.SetupGet(u => u.UserId).Returns(7);

        var handler = new CreateProjectCommandHandler(repo.Object, currentUser.Object);
        var id = await handler.Handle(new CreateProjectCommand("My Project"), CancellationToken.None);

        Assert.NotEqual(Guid.Empty, id);
        repo.Verify(r => r.AddAsync(
            It.Is<Project>(p => p.Id == id && p.Name == "My Project" && p.InsertedUserId == 7),
            It.IsAny<CancellationToken>()), Times.Once);
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
