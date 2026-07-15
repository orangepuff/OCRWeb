using Moq;
using OCRWeb.DocumentProcessing.Application.Commands.CropPdf;
using OCRWeb.DocumentProcessing.Application.Interfaces;
using OCRWeb.DocumentProcessing.Domain.Entity;
using OCRWeb.DocumentProcessing.Domain.Enums;
using OCRWeb.DocumentProcessing.Domain.Repositories;
using OCRWeb.Shared.Auditing;

namespace OCRWeb.DocumentProcessing.UnitTests;

public class CropPdfCommandHandlerTests
{
    [Fact]
    public async Task Handle_crops_source_and_persists_new_cropped_file()
    {
        var source = PdfFile.CreateOriginal(Guid.NewGuid(), "orig.pdf", "application/pdf", [1, 2, 3], userId: 1, DateTime.UtcNow);

        var repo = new Mock<IPdfFileRepository>();
        repo.Setup(r => r.GetWithContentAsync(source.Id, It.IsAny<CancellationToken>())).ReturnsAsync(source);

        var manipulator = new Mock<IPdfManipulator>();
        manipulator.Setup(m => m.Crop(It.IsAny<byte[]>(), It.IsAny<PdfCropArea>())).Returns([9, 9]);

        var currentUser = new Mock<ICurrentUser>();
        currentUser.SetupGet(u => u.UserId).Returns(7);

        var handler = new CropPdfCommandHandler(repo.Object, manipulator.Object, currentUser.Object);
        var newId = await handler.Handle(
            new CropPdfCommand(source.Id, PageNo: 1, CropX: 0, CropY: 0, Width: 10, Height: 10, FileName: null),
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, newId);
        manipulator.Verify(m => m.Crop(It.IsAny<byte[]>(), It.IsAny<PdfCropArea>()), Times.Once);
        repo.Verify(r => r.AddAsync(
            It.Is<PdfFile>(f => f.FileType == PdfFileType.Cropped && f.InsertedUserId == 7 && f.ProjectId == source.ProjectId),
            It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_missing_source_throws()
    {
        var repo = new Mock<IPdfFileRepository>();
        repo.Setup(r => r.GetWithContentAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PdfFile?)null);

        var handler = new CropPdfCommandHandler(repo.Object, Mock.Of<IPdfManipulator>(), Mock.Of<ICurrentUser>());

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new CropPdfCommand(Guid.NewGuid(), 1, 0, 0, 10, 10, null), CancellationToken.None));
    }
}
