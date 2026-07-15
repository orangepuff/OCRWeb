using Microsoft.EntityFrameworkCore;
using OCRWeb.Shared.Infrastructure.Design;

namespace OCRWeb.DocumentProcessing.Infrastructure;

/// <summary>Design-time factory for <see cref="DocumentProcessingDbContext"/> (see the shared base).</summary>
public class DocumentProcessingDbContextDesignTimeFactory
    : DesignTimeDbContextFactoryBase<DocumentProcessingDbContext>
{
    protected override string MigrationsHistorySchema => DocumentProcessingDbContext.Schema;

    protected override DocumentProcessingDbContext Create(DbContextOptions<DocumentProcessingDbContext> options)
        => new(options);
}
