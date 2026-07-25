using Moq;
using OCRWeb.Document.Application.Queries.GetPdfFileContent;
using OCRWeb.Document.Domain.Entity;
using OCRWeb.Document.Domain.Repositories;

namespace OCRWeb.Document.UnitTests;

public class GetPdfFileContentQueryHandlerTests
{
    [Fact]
    public async Task Handle_returns_streamed_content_with_metadata()
    {
        var file = PdfFile.CreateOriginal(Guid.NewGuid(), "orig.pdf", "application/pdf", [1, 2, 3], userId: 1, DateTime.UtcNow);
        using var stream = new MemoryStream([1, 2, 3]);

        var repo = new Mock<IPdfFileRepository>();
        repo.Setup(r => r.GetByIdAsync(file.Id, It.IsAny<CancellationToken>())).ReturnsAsync(file);
        repo.Setup(r => r.OpenContentStreamAsync(file.Id, It.IsAny<CancellationToken>())).ReturnsAsync(stream);

        var handler = new GetPdfFileContentQueryHandler(repo.Object);
        var result = await handler.Handle(new GetPdfFileContentQuery(file.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Same(stream, result.Content);
        Assert.Equal(file.ContentType, result.ContentType);
        Assert.Equal(file.FileName, result.FileName);
        Assert.Equal(file.SizeBytes, result.SizeBytes);
    }

    [Fact]
    public async Task Handle_returns_null_when_metadata_missing()
    {
        var repo = new Mock<IPdfFileRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((PdfFile?)null);

        var handler = new GetPdfFileContentQueryHandler(repo.Object);
        var result = await handler.Handle(new GetPdfFileContentQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
        repo.Verify(r => r.OpenContentStreamAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_returns_null_when_content_stream_missing()
    {
        var file = PdfFile.CreateOriginal(Guid.NewGuid(), "orig.pdf", "application/pdf", [1, 2, 3], userId: 1, DateTime.UtcNow);

        var repo = new Mock<IPdfFileRepository>();
        repo.Setup(r => r.GetByIdAsync(file.Id, It.IsAny<CancellationToken>())).ReturnsAsync(file);
        repo.Setup(r => r.OpenContentStreamAsync(file.Id, It.IsAny<CancellationToken>())).ReturnsAsync((Stream?)null);

        var handler = new GetPdfFileContentQueryHandler(repo.Object);
        var result = await handler.Handle(new GetPdfFileContentQuery(file.Id), CancellationToken.None);

        Assert.Null(result);
    }
}
