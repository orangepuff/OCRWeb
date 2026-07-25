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
        Assert.True(file.IsActive);
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
    public void ApplyCrop_updates_content_size_name_properties_type_and_audit()
    {
        var props = new FileProperties(1, 0, 0, 10, 10);
        var file = PdfFile.CreateOriginal(Guid.NewGuid(), "o.pdf", "application/pdf", Content, 1, DateTime.UtcNow);

        var later = DateTime.UtcNow.AddMinutes(1);
        file.ApplyCrop([9, 9, 9], "cropped.pdf", props, userId: 2, later);

        Assert.Equal(3, file.SizeBytes);
        Assert.Equal("cropped.pdf", file.FileName);
        Assert.Equal(PdfFileType.Cropped, file.FileType);
        Assert.Equal(2, file.UpdatedUserId);
        Assert.Equal(later, file.UpdatedTime);
        Assert.Equal(1, file.Properties!.PageNo);
    }

    [Fact]
    public void ApplyCrop_can_be_applied_again_to_an_already_cropped_file()
    {
        var file = PdfFile.CreateOriginal(Guid.NewGuid(), "o.pdf", "application/pdf", Content, 1, DateTime.UtcNow);
        file.ApplyCrop([9, 9, 9], "cropped.pdf", new FileProperties(1, 0, 0, 10, 10), 1, DateTime.UtcNow);

        var later = DateTime.UtcNow.AddMinutes(1);
        file.ApplyCrop([1], "cropped-again.pdf", new FileProperties(2, 5, 5, 20, 20), 2, later);

        Assert.Equal(1, file.SizeBytes);
        Assert.Equal("cropped-again.pdf", file.FileName);
        Assert.Equal(2, file.Properties!.PageNo);
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
