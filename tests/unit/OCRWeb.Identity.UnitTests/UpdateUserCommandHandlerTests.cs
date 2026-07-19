using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OCRWeb.Identity.Application.Commands.UpdateUser;
using OCRWeb.Identity.Domain.Entity;
using OCRWeb.Identity.Domain.Repositories;

namespace OCRWeb.Identity.UnitTests;

public class UpdateUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_updates_profile()
    {
        var user = new User("alice", "old@example.com", "Old Name", DateTime.UtcNow);

        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var handler = new UpdateUserCommandHandler(
            userRepo.Object, Mock.Of<ISecurityUserRuleItemRepository>(), TestHelpers.CreateTransactionLogger().Object, NullLogger<UpdateUserCommandHandler>.Instance);

        var result = await handler.Handle(new UpdateUserCommand(1, "new@example.com", "New Name", false, null), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("new@example.com", user.Email);
        Assert.Equal("New Name", user.DisplayName);
        userRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_rejects_when_a_template_is_also_given_a_parent()
    {
        var user = new User("alice", null, null, DateTime.UtcNow);

        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var handler = new UpdateUserCommandHandler(
            userRepo.Object, Mock.Of<ISecurityUserRuleItemRepository>(), TestHelpers.CreateTransactionLogger().Object, NullLogger<UpdateUserCommandHandler>.Instance);

        var result = await handler.Handle(new UpdateUserCommand(1, null, null, IsTemplateUser: true, ParentId: 5), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("template_cannot_have_parent", result.RejectionReason);
    }

    [Fact]
    public async Task Handle_rejects_becoming_a_child_when_still_has_dependents()
    {
        var user = new User("parent", null, null, DateTime.UtcNow);
        var template = new User("template", null, null, DateTime.UtcNow);
        template.MarkAsTemplateUser(DateTime.UtcNow);

        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        userRepo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(template);
        userRepo.Setup(r => r.HasChildUsersAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var handler = new UpdateUserCommandHandler(
            userRepo.Object, Mock.Of<ISecurityUserRuleItemRepository>(), TestHelpers.CreateTransactionLogger().Object, NullLogger<UpdateUserCommandHandler>.Instance);

        var result = await handler.Handle(new UpdateUserCommand(1, null, null, false, ParentId: 5), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("has_dependent_users", result.RejectionReason);
    }
}
