using OCRWeb.Document.Domain.Enums;
using OCRWeb.Document.Domain.ValueObjects;
using OrangepuffPortal.Shared.Domain;

namespace OCRWeb.Document.Domain.Entity;

/// <summary>
/// Aggregate root for a stored PDF (metadata). Maps to [docproc].[PDFFiles].
/// Holds its binary content as a 1:1 child (<see cref="PdfFileContent"/>).
/// Cross-context references (ProjectId) are by id only.
/// </summary>
public class PdfFile : AuditableEntity
{
    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long SizeBytes { get; private set; }
    public FileChecksum Checksum { get; private set; } = null!;
    public PdfFileType FileType { get; private set; }
    public FileProperties? Properties { get; private set; }
    public PdfFileContent Content { get; private set; } = null!;

    private PdfFile() { } // EF

    /// <summary>Create the original uploaded PDF.</summary>
    public static PdfFile CreateOriginal(
        Guid projectId, string fileName, string contentType, byte[] content, int userId, DateTime utcNow)
    {
        var file = NewMetadata(projectId, fileName, contentType, content, PdfFileType.Original, properties: null, userId, utcNow);
        file.Content = new PdfFileContent(file.Id, content, userId, utcNow);
        return file;
    }

    /// <summary>Create a derived PDF (Cropped/Section) produced from an original.</summary>
    public static PdfFile CreateDerived(
        Guid projectId, string fileName, string contentType, byte[] content,
        PdfFileType fileType, FileProperties properties, int userId, DateTime utcNow)
    {
        if (fileType == PdfFileType.Original)
            throw new ArgumentException("Use CreateOriginal for original files.", nameof(fileType));
        ArgumentNullException.ThrowIfNull(properties);

        var file = NewMetadata(projectId, fileName, contentType, content, fileType, properties, userId, utcNow);
        file.Content = new PdfFileContent(file.Id, content, userId, utcNow);
        return file;
    }

    /// <summary>Re-crop an existing derived file: new binary + updated crop parameters.</summary>
    public void ApplyCrop(byte[] newContent, FileProperties properties, int userId, DateTime utcNow)
    {
        if (FileType == PdfFileType.Original)
            throw new InvalidOperationException("Cannot crop the original file in place; create a derived file instead.");
        ArgumentNullException.ThrowIfNull(newContent);
        ArgumentNullException.ThrowIfNull(properties);

        Properties = properties;
        SizeBytes = newContent.LongLength;
        Checksum = FileChecksum.Compute(newContent);
        MarkUpdated(userId, utcNow);
        Content.Replace(newContent, userId, utcNow);
    }

    private static PdfFile NewMetadata(
        Guid projectId, string fileName, string contentType, byte[] content,
        PdfFileType fileType, FileProperties? properties, int userId, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(content);
        if (content.Length == 0) throw new ArgumentException("Content is empty.", nameof(content));

        var file = new PdfFile
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            FileName = SanitizeFileName(fileName),
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/pdf" : contentType,
            SizeBytes = content.LongLength,
            Checksum = FileChecksum.Compute(content),
            FileType = fileType,
            Properties = properties
        };
        file.MarkInserted(userId, utcNow);
        return file;
    }

    /// <summary>Basic display-name sanitation (plan calls for strong name handling).</summary>
    private static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name is required.", nameof(fileName));

        var cleaned = fileName.Trim();
        foreach (var c in Path.GetInvalidFileNameChars())
            cleaned = cleaned.Replace(c, '_');

        return cleaned.Length > 255 ? cleaned[..255] : cleaned;
    }
}
