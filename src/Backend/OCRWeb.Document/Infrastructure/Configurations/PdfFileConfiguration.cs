using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OCRWeb.Document.Domain.Entity;
using OCRWeb.Document.Domain.Enums;
using OCRWeb.Document.Domain.ValueObjects;

namespace OCRWeb.Document.Infrastructure.Configurations;

/// <summary>
/// Maps <see cref="PdfFile"/> to [docproc].[PDFFiles]. DB columns use the type-prefix
/// convention (s=nvarchar, bi=bigint, bin=varbinary, i=int, dt=datetime2).
/// </summary>
public class PdfFileConfiguration : IEntityTypeConfiguration<PdfFile>
{
    public void Configure(EntityTypeBuilder<PdfFile> builder)
    {
        builder.ToTable("PDFFiles");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("Id").ValueGeneratedNever();
        builder.Property(x => x.ProjectId).HasColumnName("ProjectId");

        builder.Property(x => x.FileName).HasColumnName("sFileName").HasMaxLength(255).IsRequired();
        builder.Property(x => x.ContentType).HasColumnName("sContentType").HasMaxLength(100).IsRequired();
        builder.Property(x => x.SizeBytes).HasColumnName("biSizeBytes");

        builder.Property(x => x.Checksum)
            .HasColumnName("binChecksum")
            .HasConversion(v => v.Value, v => FileChecksum.FromStored(v))
            .HasMaxLength(FileChecksum.ByteLength)
            .IsRequired();

        builder.Property(x => x.FileType)
            .HasColumnName("iFileType")
            .HasConversion<int>()
            .HasDefaultValue(PdfFileType.Original);

        // Crop/section parameters as JSON (nvarchar(max)); null for originals.
        builder.OwnsOne(x => x.Properties, owned => owned.ToJson("sFileProperties"));

        // Audit
        builder.Property(x => x.InsertedUserId).HasColumnName("iInsertedUserId");
        builder.Property(x => x.InsertedTime).HasColumnName("dtInsertedTime").HasColumnType("datetime2(3)");
        builder.Property(x => x.UpdatedUserId).HasColumnName("iUpdatedUserId");
        builder.Property(x => x.UpdatedTime).HasColumnName("dtUpdatedTime").HasColumnType("datetime2(3)");

        // 1:1 with content via shared PK (PdfFileContent.PdfFileId is PK + FK).
        builder.HasOne(x => x.Content)
            .WithOne()
            .HasForeignKey<PdfFileContent>(c => c.PdfFileId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Content).IsRequired();
    }
}
