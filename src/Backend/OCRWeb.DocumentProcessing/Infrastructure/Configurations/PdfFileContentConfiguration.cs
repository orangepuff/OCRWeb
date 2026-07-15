using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OCRWeb.DocumentProcessing.Domain.Entity;

namespace OCRWeb.DocumentProcessing.Infrastructure.Configurations;

/// <summary>
/// Maps <see cref="PdfFileContent"/> to [docproc].[PDFFileContents].
/// PdfFileId is both PK and FK (1:1 with PDFFiles). binContent is VARBINARY(MAX).
/// </summary>
public class PdfFileContentConfiguration : IEntityTypeConfiguration<PdfFileContent>
{
    public void Configure(EntityTypeBuilder<PdfFileContent> builder)
    {
        builder.ToTable("PDFFileContents");

        builder.HasKey(x => x.PdfFileId);
        builder.Property(x => x.PdfFileId).HasColumnName("PdfFileId").ValueGeneratedNever();

        builder.Property(x => x.Content).HasColumnName("binContent").IsRequired();

        builder.Property(x => x.InsertedUserId).HasColumnName("iInsertedUserId");
        builder.Property(x => x.InsertedTime).HasColumnName("dtInsertedTime").HasColumnType("datetime2(3)");
        builder.Property(x => x.UpdatedUserId).HasColumnName("iUpdatedUserId");
        builder.Property(x => x.UpdatedTime).HasColumnName("dtUpdatedTime").HasColumnType("datetime2(3)");
    }
}
