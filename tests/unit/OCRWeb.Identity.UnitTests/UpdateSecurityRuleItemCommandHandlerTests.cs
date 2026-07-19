using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OCRWeb.Identity.Application.Commands.UpdateSecurityRuleItem;
using OCRWeb.Identity.Domain.Entity;
using OCRWeb.Identity.Domain.Enums;
using OCRWeb.Identity.Domain.Repositories;
using OCRWeb.Shared.Auditing;

namespace OCRWeb.Identity.UnitTests;

public class UpdateSecurityRuleItemCommandHandlerTests
{
    [Fact]
    public async Task Handle_updates_rule_item()
    {
        var category = new SecurityRuleCategory("Document", null, 1, DateTime.UtcNow);
        var item = new SecurityRuleItem(1, "DOC_VIEW", "View documents", RuleType.Boolean, null, null, 1, DateTime.UtcNow);

        var itemRepo = new Mock<ISecurityRuleItemRepository>();
        itemRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(item);

        var categoryRepo = new Mock<ISecurityRuleCategoryRepository>();
        categoryRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(category);

        var handler = new UpdateSecurityRuleItemCommandHandler(
            itemRepo.Object, categoryRepo.Object, Mock.Of<ICurrentUser>(), TestHelpers.CreateTransactionLogger().Object, NullLogger<UpdateSecurityRuleItemCommandHandler>.Instance);

        var result = await handler.Handle(new UpdateSecurityRuleItemCommand(1, 1, "View all documents", RuleType.Integer, "DOC_VIEW_TXT", 5, true), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("View all documents", item.Description);
        Assert.Equal(RuleType.Integer, item.RuleType);
        Assert.True(item.Hidden);
    }

    [Fact]
    public async Task Handle_rejects_when_rule_item_not_found()
    {
        var itemRepo = new Mock<ISecurityRuleItemRepository>();
        itemRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync((SecurityRuleItem?)null);

        var handler = new UpdateSecurityRuleItemCommandHandler(
            itemRepo.Object, Mock.Of<ISecurityRuleCategoryRepository>(), Mock.Of<ICurrentUser>(), TestHelpers.CreateTransactionLogger().Object, NullLogger<UpdateSecurityRuleItemCommandHandler>.Instance);

        var result = await handler.Handle(new UpdateSecurityRuleItemCommand(999, 1, "X", RuleType.Boolean, null, null, false), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("rule_item_not_found", result.RejectionReason);
    }
}
