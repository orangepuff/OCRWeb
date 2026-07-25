using Moq;
using OCRWeb.Document.Application.Commands.DeletePdfFile;
using OCRWeb.Document.Domain.Entity;
using OCRWeb.Document.Domain.Repositories;
using OrangepuffPortal.Shared.Auditing;

namespace OCRWeb.Document.UnitTests;

public class DeletePdfFileCommandHandlerTests
{
    [Fact]
    public async Task Handle_removes_owned_file()
    {
        var file = PdfFile.CreateOriginal(Guid.NewGuid(), "doc.pdf", "application/pdf", [1, 2, 3], userId: 7, DateTime.UtcNow);

        var repo = new Mock<IPdfFileRepository>();
        repo.Setup(r => r.GetByIdAsync(file.Id, It.IsAny<CancellationToken>())).ReturnsAsync(file);

        var currentUser = new Mock<ICurrentUser>();
        currentUser.SetupGet(u => u.UserId).Returns(7);

        var handler = new DeletePdfFileCommandHandler(repo.Object, currentUser.Object);
        await handler.Handle(new DeletePdfFileCommand(file.Id), CancellationToken.None);

        repo.Verify(r => r.Remove(file), Times.Once);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_missing_file_throws_KeyNotFoundException()
    {
        var repo = new Mock<IPdfFileRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((PdfFile?)null);

        var handler = new DeletePdfFileCommandHandler(repo.Object, Mock.Of<ICurrentUser>());

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new DeletePdfFileCommand(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_file_owned_by_another_user_throws_UnauthorizedAccessException()
    {
        var file = PdfFile.CreateOriginal(Guid.NewGuid(), "doc.pdf", "application/pdf", [1, 2, 3], userId: 1, DateTime.UtcNow);

        var repo = new Mock<IPdfFileRepository>();
        repo.Setup(r => r.GetByIdAsync(file.Id, It.IsAny<CancellationToken>())).ReturnsAsync(file);

        var currentUser = new Mock<ICurrentUser>();
        currentUser.SetupGet(u => u.UserId).Returns(2);

        var handler = new DeletePdfFileCommandHandler(repo.Object, currentUser.Object);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new DeletePdfFileCommand(file.Id), CancellationToken.None));

        repo.Verify(r => r.Remove(It.IsAny<PdfFile>()), Times.Never);
    }
}
