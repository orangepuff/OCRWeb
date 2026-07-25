using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OCRWeb.Document.Domain.Entity;
using OCRWeb.Document.Domain.Repositories;

namespace OCRWeb.Document.Infrastructure.Repositories;

public class PdfFileRepository(DocumentDbContext db) : IPdfFileRepository
{
    public Task<PdfFile?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.PdfFiles.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<PdfFile?> GetWithContentAsync(Guid id, CancellationToken ct = default) =>
        db.PdfFiles.Include(x => x.Content).FirstOrDefaultAsync(x => x.Id == id, ct);

    // Bypasses EF Core on purpose: EF would materialize the whole blob into memory before
    // returning it, so a large file reads as a multi-second stall with zero response bytes
    // sent. SequentialAccess + GetStream() lets bytes flow to the HTTP response as they're
    // read off the wire instead.
    public async Task<Stream?> OpenContentStreamAsync(Guid id, CancellationToken ct = default)
    {
        var connection = new SqlConnection(db.Database.GetConnectionString());
        await connection.OpenAsync(ct);

        var command = connection.CreateCommand();
        command.CommandText = $"SELECT binContent FROM {DocumentDbContext.Schema}.FileContents WHERE iFileId = @id";
        command.Parameters.Add(new SqlParameter("@id", SqlDbType.UniqueIdentifier) { Value = id });

        var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess | CommandBehavior.SingleRow, ct);
        if (!await reader.ReadAsync(ct))
        {
            await reader.DisposeAsync();
            await command.DisposeAsync();
            await connection.DisposeAsync();
            return null;
        }

        return new SqlBlobStream(connection, command, reader, reader.GetStream(0));
    }

    public async Task<IReadOnlyList<PdfFile>> ListByProjectAsync(Guid projectId, CancellationToken ct = default) =>
        await db.PdfFiles
            .Where(x => x.ParentId == projectId && x.IsActive)
            .OrderByDescending(x => x.InsertedTime)
            .ToListAsync(ct);

    public async Task AddAsync(PdfFile file, CancellationToken ct = default) =>
        await db.PdfFiles.AddAsync(file, ct);

    public void Remove(PdfFile file) =>
        db.PdfFiles.Remove(file);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);

    public Task<int> RemoveAllByParentIdAsync(Guid parentId, CancellationToken ct = default) =>
        db.PdfFiles.Where(x => x.ParentId == parentId).ExecuteDeleteAsync(ct);
}
