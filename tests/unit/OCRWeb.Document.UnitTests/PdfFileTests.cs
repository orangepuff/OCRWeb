using OCRWeb.Document.Domain.Entity;
using OCRWeb.Document.Domain.Enums;
using OCRWeb.Document.Domain.ValueObjects;

namespace OCRWeb.Document.UnitTests;

public class PdfFileTests
{
    private static readonly byte[] Content = [1, 2, 3, 4];

    [Fact]
    public void CreateOriginal_sets_original_type_and_audit()
    {
        var now = DateTime.UtcNow;
        var file = PdfFile.CreateOriginal(Guid.NewGuid(), "doc.pdf", "application/pdf", Content, userId: 1, now);

        Assert.Equal(PdfFileType.Original, file.FileType);
        Assert.Null(file.Properties);
        Assert.Equal(Content.LongLength, file.SizeBytes);
        Assert.NotNull(file.Content);
        Assert.Equal(1, file.InsertedUserId);
        Assert.Equal(now, file.InsertedTime);
    }

    [Fact]
    public void CreateOriginal_sanitizes_invalid_filename_chars()
    {
        var file = PdfFile.CreateOriginal(Guid.NewGuid(), "a:b*c.pdf", "application/pdf", Content, 1, DateTime.UtcNow);

        Assert.DoesNotContain(':', file.FileName);
        Assert.DoesNotContain('*', file.FileName);
    }

    [Fact]
    public void CreateOriginal_empty_content_throws()
    {
        Assert.Throws<ArgumentException>(() =>
            PdfFile.CreateOriginal(Guid.NewGuid(), "doc.pdf", "application/pdf", [], 1, DateTime.UtcNow));
    }

    [Fact]
    public void CreateDerived_requires_properties()
    {
        Assert.Throws<ArgumentNullException>(() =>
            PdfFile.CreateDerived(Guid.NewGuid(), "c.pdf", "application/pdf", Content,
                PdfFileType.Cropped, properties: null!, 1, DateTime.UtcNow));
    }

    [Fact]
    public void CreateDerived_rejects_original_type()
    {
        var props = new FileProperties(1, 0, 0, 10, 10);
        Assert.Throws<ArgumentException>(() =>
            PdfFile.CreateDerived(Guid.NewGuid(), "c.pdf", "application/pdf", Content,
                PdfFileType.Original, props, 1, DateTime.UtcNow));
    }

    [Fact]
    public void ApplyCrop_updates_content_size_properties_and_audit()
    {
        var props = new FileProperties(1, 0, 0, 10, 10);
        var file = PdfFile.CreateDerived(Guid.NewGuid(), "c.pdf", "application/pdf", Content,
            PdfFileType.Cropped, props, 1, DateTime.UtcNow);

        var later = DateTime.UtcNow.AddMinutes(1);
        file.ApplyCrop([9, 9, 9], new FileProperties(2, 5, 5, 20, 20), userId: 2, later);

        Assert.Equal(3, file.SizeBytes);
        Assert.Equal(2, file.UpdatedUserId);
        Assert.Equal(later, file.UpdatedTime);
        Assert.Equal(2, file.Properties!.PageNo);
    }

    [Fact]
    public void ApplyCrop_on_original_throws()
    {
        var file = PdfFile.CreateOriginal(Guid.NewGuid(), "o.pdf", "application/pdf", Content, 1, DateTime.UtcNow);
        Assert.Throws<InvalidOperationException>(() =>
            file.ApplyCrop([1], new FileProperties(1, 0, 0, 10, 10), 1, DateTime.UtcNow));
    }

    [Theory]
    [InlineData(0, 0, 0, 10, 10)] // page < 1
    [InlineData(1, 0, 0, 0, 10)]  // width <= 0
    [InlineData(1, 0, 0, 10, 0)]  // height <= 0
    public void FileProperties_invalid_values_throw(int page, int x, int y, int w, int h)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new FileProperties(page, x, y, w, h));
    }
}
