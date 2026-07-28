using OCRWeb.Document.Domain.Enums;
using OCRWeb.Document.Domain.ValueObjects;
using OrangepuffPortal.Shared.Domain;

namespace OCRWeb.Document.Domain.Entity;

/// <summary>
/// Aggregate root for a stored PDF (metadata). Maps to [docproc].[Files].
/// Holds its binary content as a 1:1 child (<see cref="PdfFileContent"/>).
/// Cross-context references (ParentId) are by id only - the id happens to be a project id
/// today, but this context doesn't model "Project" as a concept of its own.
/// </summary>
public class PdfFile : AuditableEntity
{
    public int Id { get; private set; }
    public int ParentId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long SizeBytes { get; private set; }
    public FileChecksum Checksum { get; private set; } = null!;
    public PdfFileType FileType { get; private set; }
    public FileProperties? Properties { get; private set; }
    public bool IsActive { get; private set; } = true;
    public PdfFileContent Content { get; private set; } = null!;

    private PdfFile() { } // EF

    /// <summary>
    /// Create the original uploaded PDF.
    /// </summary>
    public static PdfFile CreateOriginal(int parentId, string fileName, string contentType, byte[] content, int userId, DateTime utcNow)
    {
        var file = NewMetadata(parentId, fileName, contentType, content, PdfFileType.Original, properties: null, userId, utcNow);
        file.Content = new PdfFileContent(content, userId, utcNow);
        return file;
    }

    /// <summary>
    /// Replace this file's content in place - used by crop, which always mutates the
    /// existing file rather than creating a separate derived one.
    /// </summary>
    public void ApplyCrop(byte[] newContent, string fileName, FileProperties properties, int userId, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(newContent);
        ArgumentNullException.ThrowIfNull(properties);

        FileName = SanitizeFileName(fileName);
        Properties = properties;
        FileType = PdfFileType.Cropped;
        SizeBytes = newContent.LongLength;
        Checksum = FileChecksum.Compute(newContent);
        MarkUpdated(userId, utcNow);
        Content.Replace(newContent, userId, utcNow);
    }

    private static PdfFile NewMetadata(
        int parentId, string fileName, string contentType, byte[] content,
        PdfFileType fileType, FileProperties? properties, int userId, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(content);
        if (content.Length == 0)
        {
            throw new ArgumentException("Content is empty.", nameof(content));
        }

        var file = new PdfFile
        {
            ParentId = parentId,
            FileName = SanitizeFileName(fileName),
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/pdf" : contentType,
            SizeBytes = content.LongLength,
            Checksum = FileChecksum.Compute(content),
            FileType = fileType,
            Properties = properties,
            IsActive = true
        };
        file.MarkInserted(userId, utcNow);
        return file;
    }

    // Fixed set rather than Path.GetInvalidFileNameChars(): that API is OS-dependent (Linux only
    // rejects '\0' and '/'), so sanitizing against it would let ':'/'*'/etc. through when the app
    // runs in a Linux container even though they're invalid on Windows, where names get downloaded to.
    private static readonly char[] InvalidFileNameChars = ['"', '<', '>', ':', '|', '?', '*', '\\', '/'];

    /// <summary>Basic display-name sanitation (plan calls for strong name handling).</summary>
    private static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name is required.", nameof(fileName));
        }

        var cleaned = fileName.Trim();
        foreach (var c in InvalidFileNameChars)
        {
            cleaned = cleaned.Replace(c, '_');
        }

        return cleaned.Length > 255 ? cleaned[..255] : cleaned;
    }
}
