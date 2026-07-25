using Moq;
using OCRWeb.Document.Application.Commands.CropPdf;
using OCRWeb.Pdf.Contract;
using OCRWeb.Document.Domain.Entity;
using OCRWeb.Document.Domain.Enums;
using OCRWeb.Document.Domain.Repositories;
using OrangepuffPortal.Shared.Auditing;

namespace OCRWeb.Document.UnitTests;

public class CropPdfCommandHandlerTests
{
    [Fact]
    public async Task Handle_replaces_source_content_in_place()
    {
        var source = PdfFile.CreateOriginal(Guid.NewGuid(), "orig.pdf", "application/pdf", [1, 2, 3], userId: 1, DateTime.UtcNow);

        var repo = new Mock<IPdfFileRepository>();
        repo.Setup(r => r.GetWithContentAsync(source.Id, It.IsAny<CancellationToken>())).ReturnsAsync(source);

        var manipulator = new Mock<IPdfManipulator>();
        manipulator.Setup(m => m.Crop(It.IsAny<byte[]>(), It.IsAny<PdfCropArea>())).Returns([9, 9]);

        var currentUser = new Mock<ICurrentUser>();
        currentUser.SetupGet(u => u.UserId).Returns(7);

        var handler = new CropPdfCommandHandler(repo.Object, manipulator.Object, currentUser.Object);
        var resultId = await handler.Handle(
            new CropPdfCommand(source.Id, PageNo: 1, CropX: 0, CropY: 0, Width: 10, Height: 10, FileName: null),
            CancellationToken.None);

        // Crop replaces the source file's own content - same id, no new file created/removed.
        Assert.Equal(source.Id, resultId);
        Assert.Equal(PdfFileType.Cropped, source.FileType);
        Assert.Equal(2, source.SizeBytes);
        manipulator.Verify(m => m.Crop(It.IsAny<byte[]>(), It.IsAny<PdfCropArea>()), Times.Once);
        repo.Verify(r => r.AddAsync(It.IsAny<PdfFile>(), It.IsAny<CancellationToken>()), Times.Never);
        repo.Verify(r => r.Remove(It.IsAny<PdfFile>()), Times.Never);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_falls_back_to_source_name_when_no_filename_given()
    {
        var source = PdfFile.CreateOriginal(Guid.NewGuid(), "orig.pdf", "application/pdf", [1, 2, 3], userId: 1, DateTime.UtcNow);

        var repo = new Mock<IPdfFileRepository>();
        repo.Setup(r => r.GetWithContentAsync(source.Id, It.IsAny<CancellationToken>())).ReturnsAsync(source);

        var manipulator = new Mock<IPdfManipulator>();
        manipulator.Setup(m => m.Crop(It.IsAny<byte[]>(), It.IsAny<PdfCropArea>())).Returns([9, 9]);

        var handler = new CropPdfCommandHandler(repo.Object, manipulator.Object, Mock.Of<ICurrentUser>());
        await handler.Handle(
            new CropPdfCommand(source.Id, PageNo: 1, CropX: 0, CropY: 0, Width: 10, Height: 10, FileName: "renamed.pdf"),
            CancellationToken.None);

        Assert.Equal("renamed.pdf", source.FileName);
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
