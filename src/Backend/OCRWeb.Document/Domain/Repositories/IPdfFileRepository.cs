using OCRWeb.Document.Domain.Entity;

namespace OCRWeb.Document.Domain.Repositories;

public interface IPdfFileRepository
{
    /// <summary>Metadata only (no binary content).</summary>
    Task<PdfFile?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Metadata + binary content (for crop/download).</summary>
    Task<PdfFile?> GetWithContentAsync(Guid id, CancellationToken ct = default);

    /// <summary>Metadata list for a project (no binary content).</summary>
    Task<IReadOnlyList<PdfFile>> ListByProjectAsync(Guid projectId, CancellationToken ct = default);

    Task AddAsync(PdfFile file, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
