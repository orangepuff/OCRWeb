using Microsoft.EntityFrameworkCore;
using OCRWeb.DocumentProcessing.Domain.Entity;
using OCRWeb.DocumentProcessing.Domain.Repositories;

namespace OCRWeb.DocumentProcessing.Infrastructure.Repositories;

public class PdfFileRepository(DocumentProcessingDbContext db) : IPdfFileRepository
{
    public Task<PdfFile?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.PdfFiles.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<PdfFile?> GetWithContentAsync(Guid id, CancellationToken ct = default) =>
        db.PdfFiles.Include(x => x.Content).FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<PdfFile>> ListByProjectAsync(Guid projectId, CancellationToken ct = default) =>
        await db.PdfFiles
            .Where(x => x.ProjectId == projectId)
            .OrderByDescending(x => x.InsertedTime)
            .ToListAsync(ct);

    public async Task AddAsync(PdfFile file, CancellationToken ct = default) =>
        await db.PdfFiles.AddAsync(file, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);
}
