using OrangepuffPortal.Shared.Domain;

namespace OCRWeb.Document.Domain.Entity;

/// <summary>
/// Binary content of a PDF, split from metadata so listing/querying never drags the blob.
/// 1:1 with <see cref="PdfFile"/> via FileId (a unique FK; the table's own PK is a separate
/// technical identity column not modeled here - see PdfFileContentConfiguration).
/// Part of the PdfFile aggregate — only created/mutated through the aggregate root.
/// </summary>
public class PdfFileContent : AuditableEntity
{
    public Guid FileId { get; private set; }
    public byte[] Content { get; private set; } = [];

    private PdfFileContent() { } // EF

    internal PdfFileContent(Guid fileId, byte[] content, int userId, DateTime utcNow)
    {
        FileId = fileId;
        Content = content ?? throw new ArgumentNullException(nameof(content));
        MarkInserted(userId, utcNow);
    }

    internal void Replace(byte[] content, int userId, DateTime utcNow)
    {
        Content = content ?? throw new ArgumentNullException(nameof(content));
        MarkUpdated(userId, utcNow);
    }
}
