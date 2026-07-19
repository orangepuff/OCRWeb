using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OCRWeb.Identity.Application.Commands.AddSecurityRuleItem;
using OCRWeb.Identity.Domain.Entity;
using OCRWeb.Identity.Domain.Enums;
using OCRWeb.Identity.Domain.Repositories;
using OCRWeb.Shared.Auditing;

namespace OCRWeb.Identity.UnitTests;

public class AddSecurityRuleItemCommandHandlerTests
{
    [Fact]
    public async Task Handle_creates_rule_item()
    {
        var category = new SecurityRuleCategory("Document", null, 1, DateTime.UtcNow);

        var itemRepo = new Mock<ISecurityRuleItemRepository>();
        itemRepo.Setup(r => r.GetByCodeAsync("DOC_VIEW", It.IsAny<CancellationToken>())).ReturnsAsync((SecurityRuleItem?)null);

        var categoryRepo = new Mock<ISecurityRuleCategoryRepository>();
        categoryRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(category);

        var currentUser = new Mock<ICurrentUser>();
        currentUser.SetupGet(u => u.UserId).Returns(7);

        var handler = new AddSecurityRuleItemCommandHandler(
            itemRepo.Object, categoryRepo.Object, currentUser.Object, TestHelpers.CreateTransactionLogger().Object, NullLogger<AddSecurityRuleItemCommandHandler>.Instance);

        var result = await handler.Handle(new AddSecurityRuleItemCommand(1, "DOC_VIEW", "View documents", RuleType.Boolean, null, null), CancellationToken.None);

        Assert.True(result.Success);
        itemRepo.Verify(r => r.AddAsync(
            It.Is<SecurityRuleItem>(i => i.Code == "DOC_VIEW" && i.CategoryId == 1 && i.InsertedUserId == 7), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_rejects_when_category_not_found()
    {
        var itemRepo = new Mock<ISecurityRuleItemRepository>();
        var categoryRepo = new Mock<ISecurityRuleCategoryRepository>();
        categoryRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync((SecurityRuleCategory?)null);

        var handler = new AddSecurityRuleItemCommandHandler(
            itemRepo.Object, categoryRepo.Object, Mock.Of<ICurrentUser>(), TestHelpers.CreateTransactionLogger().Object, NullLogger<AddSecurityRuleItemCommandHandler>.Instance);

        var result = await handler.Handle(new AddSecurityRuleItemCommand(999, "DOC_VIEW", "View documents", RuleType.Boolean, null, null), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("category_not_found", result.RejectionReason);
    }

    [Fact]
    public async Task Handle_rejects_duplicate_code()
    {
        var category = new SecurityRuleCategory("Document", null, 1, DateTime.UtcNow);

        var itemRepo = new Mock<ISecurityRuleItemRepository>();
        itemRepo.Setup(r => r.GetByCodeAsync("DOC_VIEW", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SecurityRuleItem(1, "DOC_VIEW", "View documents", RuleType.Boolean, null, null, 1, DateTime.UtcNow));

        var categoryRepo = new Mock<ISecurityRuleCategoryRepository>();
        categoryRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(category);

        var handler = new AddSecurityRuleItemCommandHandler(
            itemRepo.Object, categoryRepo.Object, Mock.Of<ICurrentUser>(), TestHelpers.CreateTransactionLogger().Object, NullLogger<AddSecurityRuleItemCommandHandler>.Instance);

        var result = await handler.Handle(new AddSecurityRuleItemCommand(1, "DOC_VIEW", "View documents", RuleType.Boolean, null, null), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("code_taken", result.RejectionReason);
    }
}
