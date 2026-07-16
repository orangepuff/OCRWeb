using Microsoft.EntityFrameworkCore;
using OCRWeb.Shared.Infrastructure.Design;

namespace OCRWeb.Document.Infrastructure;

/// <summary>Design-time factory for <see cref="DocumentDbContext"/> (see the shared base).</summary>
public class DocumentDbContextDesignTimeFactory
    : DesignTimeDbContextFactoryBase<DocumentDbContext>
{
    protected override string MigrationsHistorySchema => DocumentDbContext.Schema;

    protected override DocumentDbContext Create(DbContextOptions<DocumentDbContext> options)
        => new(options);
}
