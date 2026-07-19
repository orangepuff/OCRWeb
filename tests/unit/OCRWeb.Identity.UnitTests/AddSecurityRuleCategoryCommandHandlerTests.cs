using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OCRWeb.Identity.Application.Commands.AddSecurityRuleCategory;
using OCRWeb.Identity.Domain.Entity;
using OCRWeb.Identity.Domain.Repositories;
using OCRWeb.Shared.Auditing;

namespace OCRWeb.Identity.UnitTests;

public class AddSecurityRuleCategoryCommandHandlerTests
{
    [Fact]
    public async Task Handle_creates_category()
    {
        var repo = new Mock<ISecurityRuleCategoryRepository>();
        repo.Setup(r => r.GetByDescAsync("Document", It.IsAny<CancellationToken>())).ReturnsAsync((SecurityRuleCategory?)null);

        var currentUser = new Mock<ICurrentUser>();
        currentUser.SetupGet(u => u.UserId).Returns(7);

        var handler = new AddSecurityRuleCategoryCommandHandler(
            repo.Object, currentUser.Object, TestHelpers.CreateTransactionLogger().Object, NullLogger<AddSecurityRuleCategoryCommandHandler>.Instance);

        var result = await handler.Handle(new AddSecurityRuleCategoryCommand("Document", null), CancellationToken.None);

        Assert.True(result.Success);
        repo.Verify(r => r.AddAsync(
            It.Is<SecurityRuleCategory>(c => c.CategoryDesc == "Document" && c.InsertedUserId == 7), It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_rejects_duplicate_category_desc()
    {
        var repo = new Mock<ISecurityRuleCategoryRepository>();
        repo.Setup(r => r.GetByDescAsync("Document", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SecurityRuleCategory("Document", null, 1, DateTime.UtcNow));

        var handler = new AddSecurityRuleCategoryCommandHandler(
            repo.Object, Mock.Of<ICurrentUser>(), TestHelpers.CreateTransactionLogger().Object, NullLogger<AddSecurityRuleCategoryCommandHandler>.Instance);

        var result = await handler.Handle(new AddSecurityRuleCategoryCommand("Document", null), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("category_desc_taken", result.RejectionReason);
    }
}
