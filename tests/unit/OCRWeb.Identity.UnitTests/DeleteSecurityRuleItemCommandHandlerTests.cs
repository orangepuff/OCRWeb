using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OCRWeb.Identity.Application.Commands.DeleteSecurityRuleItem;
using OCRWeb.Identity.Domain.Entity;
using OCRWeb.Identity.Domain.Enums;
using OCRWeb.Identity.Domain.Repositories;

namespace OCRWeb.Identity.UnitTests;

public class DeleteSecurityRuleItemCommandHandlerTests
{
    [Fact]
    public async Task Handle_deletes_rule_item_with_no_assignments()
    {
        var item = new SecurityRuleItem(1, "DOC_VIEW", "View documents", RuleType.Boolean, null, null, 1, DateTime.UtcNow);

        var itemRepo = new Mock<ISecurityRuleItemRepository>();
        itemRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(item);

        var userRuleItemRepo = new Mock<ISecurityUserRuleItemRepository>();
        userRuleItemRepo.Setup(r => r.HasAnyForRuleItemAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var handler = new DeleteSecurityRuleItemCommandHandler(
            itemRepo.Object, userRuleItemRepo.Object, TestHelpers.CreateTransactionLogger().Object, NullLogger<DeleteSecurityRuleItemCommandHandler>.Instance);

        var result = await handler.Handle(new DeleteSecurityRuleItemCommand(1), CancellationToken.None);

        Assert.True(result.Success);
        itemRepo.Verify(r => r.DeleteAsync(item, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_rejects_when_rule_item_still_has_assignments()
    {
        var item = new SecurityRuleItem(1, "DOC_VIEW", "View documents", RuleType.Boolean, null, null, 1, DateTime.UtcNow);

        var itemRepo = new Mock<ISecurityRuleItemRepository>();
        itemRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(item);

        var userRuleItemRepo = new Mock<ISecurityUserRuleItemRepository>();
        userRuleItemRepo.Setup(r => r.HasAnyForRuleItemAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var handler = new DeleteSecurityRuleItemCommandHandler(
            itemRepo.Object, userRuleItemRepo.Object, TestHelpers.CreateTransactionLogger().Object, NullLogger<DeleteSecurityRuleItemCommandHandler>.Instance);

        var result = await handler.Handle(new DeleteSecurityRuleItemCommand(1), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("has_assignments", result.RejectionReason);
        itemRepo.Verify(r => r.DeleteAsync(It.IsAny<SecurityRuleItem>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
