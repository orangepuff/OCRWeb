using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OCRWeb.Identity.Application.Commands.DeleteUser;
using OCRWeb.Identity.Domain.Entity;
using OCRWeb.Identity.Domain.Repositories;

namespace OCRWeb.Identity.UnitTests;

public class DeleteUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_deletes_user_with_no_dependents()
    {
        var user = new User("alice", null, null, DateTime.UtcNow);

        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        repo.Setup(r => r.HasChildUsersAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var handler = new DeleteUserCommandHandler(repo.Object, TestHelpers.CreateTransactionLogger().Object, NullLogger<DeleteUserCommandHandler>.Instance);

        var result = await handler.Handle(new DeleteUserCommand(1), CancellationToken.None);

        Assert.True(result.Success);
        repo.Verify(r => r.DeleteAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_rejects_when_user_is_still_used_as_a_template()
    {
        var user = new User("template", null, null, DateTime.UtcNow);

        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        repo.Setup(r => r.HasChildUsersAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var handler = new DeleteUserCommandHandler(repo.Object, TestHelpers.CreateTransactionLogger().Object, NullLogger<DeleteUserCommandHandler>.Instance);

        var result = await handler.Handle(new DeleteUserCommand(1), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("has_dependent_users", result.RejectionReason);
        repo.Verify(r => r.DeleteAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_rejects_when_user_not_found()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var handler = new DeleteUserCommandHandler(repo.Object, TestHelpers.CreateTransactionLogger().Object, NullLogger<DeleteUserCommandHandler>.Instance);

        var result = await handler.Handle(new DeleteUserCommand(999), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("user_not_found", result.RejectionReason);
    }
}
