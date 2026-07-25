using OCRWeb.Document.Domain.Entity;

namespace OCRWeb.Document.Domain.Repositories;

public interface IPdfFileRepository
{
    /// <summary>Metadata only (no binary content).</summary>
    Task<PdfFile?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Metadata + binary content (for crop, which needs the full byte[] in memory).</summary>
    Task<PdfFile?> GetWithContentAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Opens a stream over the binary content without loading it into memory first.
    /// Returns null if no file with this id exists. Caller must dispose the stream.
    /// </summary>
    Task<Stream?> OpenContentStreamAsync(int id, CancellationToken ct = default);

    /// <summary>Metadata list for a project (no binary content). Excludes inactive files.</summary>
    Task<IReadOnlyList<PdfFile>> ListByProjectAsync(int projectId, CancellationToken ct = default);

    Task AddAsync(PdfFile file, CancellationToken ct = default);
    void Remove(PdfFile file);
    Task SaveChangesAsync(CancellationToken ct = default);

    /// <summary>
    /// Hard-deletes every file for a parent (project), active or not - content cascades via the
    /// FK. Used when the parent itself is deleted, which is a full teardown, not a listing query.
    /// </summary>
    Task<int> RemoveAllByParentIdAsync(int parentId, CancellationToken ct = default);
}
