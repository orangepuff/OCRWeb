using OrangepuffPortal.Shared.Domain;

namespace OCRWeb.Document.Domain.Entity;

/// <summary>
/// Binary content of a PDF, split from metadata so listing/querying never drags the blob.
/// 1:1 with <see cref="PdfFile"/> via FileId (a unique FK; the table's own PK is a separate
/// technical identity column not modeled here - see PdfFileContentConfiguration). FileId isn't
/// set here: PdfFile.Id is DB-generated (identity), so EF fills it in via the relationship
/// (PdfFile.Content navigation) once both are saved together - it can't be known beforehand.
/// Part of the PdfFile aggregate — only created/mutated through the aggregate root.
/// </summary>
public class PdfFileContent : AuditableEntity
{
    public int FileId { get; private set; }
    public byte[] Content { get; private set; } = [];

    private PdfFileContent() { } // EF

    internal PdfFileContent(byte[] content, int userId, DateTime utcNow)
    {
        Content = content ?? throw new ArgumentNullException(nameof(content));
        MarkInserted(userId, utcNow);
    }

    internal void Replace(byte[] content, int userId, DateTime utcNow)
    {
        Content = content ?? throw new ArgumentNullException(nameof(content));
        MarkUpdated(userId, utcNow);
    }
}
