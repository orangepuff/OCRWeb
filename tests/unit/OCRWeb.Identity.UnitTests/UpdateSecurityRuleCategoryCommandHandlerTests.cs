using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OCRWeb.Identity.Application.Commands.UpdateSecurityRuleCategory;
using OCRWeb.Identity.Domain.Entity;
using OCRWeb.Identity.Domain.Repositories;
using OCRWeb.Shared.Auditing;

namespace OCRWeb.Identity.UnitTests;

public class UpdateSecurityRuleCategoryCommandHandlerTests
{
    [Fact]
    public async Task Handle_updates_category()
    {
        var category = new SecurityRuleCategory("Document", null, 1, DateTime.UtcNow);

        var repo = new Mock<ISecurityRuleCategoryRepository>();
        repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        repo.Setup(r => r.GetByDescAsync("Document Management", It.IsAny<CancellationToken>())).ReturnsAsync((SecurityRuleCategory?)null);

        var handler = new UpdateSecurityRuleCategoryCommandHandler(
            repo.Object, Mock.Of<ICurrentUser>(), TestHelpers.CreateTransactionLogger().Object, NullLogger<UpdateSecurityRuleCategoryCommandHandler>.Instance);

        var result = await handler.Handle(new UpdateSecurityRuleCategoryCommand(1, "Document Management", "DOC", true), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("Document Management", category.CategoryDesc);
        Assert.True(category.Hidden);
    }

    [Fact]
    public async Task Handle_rejects_when_category_not_found()
    {
        var repo = new Mock<ISecurityRuleCategoryRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync((SecurityRuleCategory?)null);

        var handler = new UpdateSecurityRuleCategoryCommandHandler(
            repo.Object, Mock.Of<ICurrentUser>(), TestHelpers.CreateTransactionLogger().Object, NullLogger<UpdateSecurityRuleCategoryCommandHandler>.Instance);

        var result = await handler.Handle(new UpdateSecurityRuleCategoryCommand(999, "X", null, false), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("category_not_found", result.RejectionReason);
    }
}
