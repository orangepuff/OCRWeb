using Microsoft.EntityFrameworkCore;
using OrangepuffPortal.Shared.Infrastructure.Design;

namespace OCRWeb.OCR.Infrastructure;

/// <summary>Design-time factory for <see cref="OcrDbContext"/> (see the shared base).</summary>
public class OcrDbContextDesignTimeFactory : DesignTimeDbContextFactoryBase<OcrDbContext>
{
    protected override string MigrationsHistorySchema => OcrDbContext.Schema;

    protected override OcrDbContext Create(DbContextOptions<OcrDbContext> options) => new(options);
}
