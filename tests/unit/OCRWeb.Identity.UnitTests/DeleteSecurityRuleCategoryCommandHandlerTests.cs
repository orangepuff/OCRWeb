using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OCRWeb.Identity.Application.Commands.DeleteSecurityRuleCategory;
using OCRWeb.Identity.Domain.Entity;
using OCRWeb.Identity.Domain.Repositories;

namespace OCRWeb.Identity.UnitTests;

public class DeleteSecurityRuleCategoryCommandHandlerTests
{
    [Fact]
    public async Task Handle_deletes_category_with_no_items()
    {
        var category = new SecurityRuleCategory("Document", null, 1, DateTime.UtcNow);

        var repo = new Mock<ISecurityRuleCategoryRepository>();
        repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        repo.Setup(r => r.HasItemsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var handler = new DeleteSecurityRuleCategoryCommandHandler(repo.Object, TestHelpers.CreateTransactionLogger().Object, NullLogger<DeleteSecurityRuleCategoryCommandHandler>.Instance);

        var result = await handler.Handle(new DeleteSecurityRuleCategoryCommand(1), CancellationToken.None);

        Assert.True(result.Success);
        repo.Verify(r => r.DeleteAsync(category, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_rejects_when_category_still_has_items()
    {
        var category = new SecurityRuleCategory("Document", null, 1, DateTime.UtcNow);

        var repo = new Mock<ISecurityRuleCategoryRepository>();
        repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        repo.Setup(r => r.HasItemsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var handler = new DeleteSecurityRuleCategoryCommandHandler(repo.Object, TestHelpers.CreateTransactionLogger().Object, NullLogger<DeleteSecurityRuleCategoryCommandHandler>.Instance);

        var result = await handler.Handle(new DeleteSecurityRuleCategoryCommand(1), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("has_items", result.RejectionReason);
        repo.Verify(r => r.DeleteAsync(It.IsAny<SecurityRuleCategory>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
