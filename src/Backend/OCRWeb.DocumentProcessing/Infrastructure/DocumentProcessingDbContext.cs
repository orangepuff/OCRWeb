using Microsoft.EntityFrameworkCore;
using OCRWeb.DocumentProcessing.Domain.Entity;
using OCRWeb.DocumentProcessing.Infrastructure.Configurations;

namespace OCRWeb.DocumentProcessing.Infrastructure;

/// <summary>
/// EF Core context for the Document Processing bounded context. Owns the [docproc] schema.
/// </summary>
public class DocumentProcessingDbContext(DbContextOptions<DocumentProcessingDbContext> options)
    : DbContext(options)
{
    public const string Schema = "docproc";

    public DbSet<PdfFile> PdfFiles => Set<PdfFile>();
    public DbSet<PdfFileContent> PdfFileContents => Set<PdfFileContent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfiguration(new PdfFileConfiguration());
        modelBuilder.ApplyConfiguration(new PdfFileContentConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
