using Microsoft.EntityFrameworkCore;
using OCRWeb.Document.Domain.Entity;
using OCRWeb.Document.Infrastructure.Configurations;

namespace OCRWeb.Document.Infrastructure;

/// <summary>
/// EF Core context for the Document Processing bounded context. Owns the [docproc] schema.
/// </summary>
public class DocumentDbContext(DbContextOptions<DocumentDbContext> options)
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
