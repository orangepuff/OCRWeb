using Microsoft.EntityFrameworkCore;

namespace OCRWeb.OCR.Infrastructure;

/// <summary>
/// EF Core context for the OCR bounded context. Owns the [ocr] schema.
/// No aggregates are mapped yet — add entity configurations here as the module grows,
/// then create the first migration (see README).
/// </summary>
public class OcrDbContext(DbContextOptions<OcrDbContext> options)
    : DbContext(options)
{
    public const string Schema = "ocr";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        base.OnModelCreating(modelBuilder);
    }
}
