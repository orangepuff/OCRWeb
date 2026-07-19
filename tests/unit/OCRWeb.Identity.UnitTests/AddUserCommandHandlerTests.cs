using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OCRWeb.Identity.Application.Commands.AddUser;
using OCRWeb.Identity.Domain.Entity;
using OCRWeb.Identity.Domain.Repositories;

namespace OCRWeb.Identity.UnitTests;

public class AddUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_creates_independent_user()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByUsernameAsync("newuser", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var handler = new AddUserCommandHandler(repo.Object, TestHelpers.CreateTransactionLogger().Object, NullLogger<AddUserCommandHandler>.Instance);

        var result = await handler.Handle(new AddUserCommand("newuser", "new@example.com", "New User", null), CancellationToken.None);

        Assert.True(result.Success);
        repo.Verify(r => r.AddAsync(It.Is<User>(u => u.Username == "newuser" && u.Email == "new@example.com"), It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_rejects_when_username_taken()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByUsernameAsync("taken", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User("taken", null, null, DateTime.UtcNow));

        var handler = new AddUserCommandHandler(repo.Object, TestHelpers.CreateTransactionLogger().Object, NullLogger<AddUserCommandHandler>.Instance);

        var result = await handler.Handle(new AddUserCommand("taken", null, null, null), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("username_taken", result.RejectionReason);
        repo.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_rejects_when_template_user_is_not_usable()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByUsernameAsync("newuser", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        repo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User("notatemplate", null, null, DateTime.UtcNow)); // IsTemplateUser is false

        var handler = new AddUserCommandHandler(repo.Object, TestHelpers.CreateTransactionLogger().Object, NullLogger<AddUserCommandHandler>.Instance);

        var result = await handler.Handle(new AddUserCommand("newuser", null, null, 99), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("not_a_template_user", result.RejectionReason);
    }
}
