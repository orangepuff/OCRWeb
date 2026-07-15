using OCRWeb.Shared.Domain;

namespace OCRWeb.DocumentProcessing.Domain.Entity;

/// <summary>
/// Binary content of a PDF, split from metadata so listing/querying never drags the blob.
/// 1:1 with <see cref="PdfFile"/> via a shared primary key (PdfFileId is PK + FK).
/// Part of the PdfFile aggregate — only created/mutated through the aggregate root.
/// </summary>
public class PdfFileContent : AuditableEntity
{
    public Guid PdfFileId { get; private set; }
    public byte[] Content { get; private set; } = [];

    private PdfFileContent() { } // EF

    internal PdfFileContent(Guid pdfFileId, byte[] content, int userId, DateTime utcNow)
    {
        PdfFileId = pdfFileId;
        Content = content ?? throw new ArgumentNullException(nameof(content));
        MarkInserted(userId, utcNow);
    }

    internal void Replace(byte[] content, int userId, DateTime utcNow)
    {
        Content = content ?? throw new ArgumentNullException(nameof(content));
        MarkUpdated(userId, utcNow);
    }
}
