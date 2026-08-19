using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OCRWeb.Document.Domain.Entity;

namespace OCRWeb.Document.Infrastructure.Configurations;

/// <summary>
/// Maps <see cref="PdfFileContent"/> to [docproc].[FileContents].
/// The table's PK is a technical identity column (iId) not modeled on the domain entity -
/// nothing in the domain needs it, the aggregate is always addressed via FileId. iFileId is a
/// unique FK to [docproc].[Files] instead, preserving the 1:1 relationship. binContent is VARBINARY(MAX).
/// </summary>
public class PdfFileContentConfiguration : IEntityTypeConfiguration<PdfFileContent>
{
    public void Configure(EntityTypeBuilder<PdfFileContent> builder)
    {
        builder.ToTable("FileContents");

        builder.Property<int>("Id").HasColumnName("iId").ValueGeneratedOnAdd();
        builder.HasKey("Id");

        builder.Property(x => x.FileId).HasColumnName("iFileId");
        builder.HasIndex(x => x.FileId).IsUnique();

        builder.Property(x => x.Content).HasColumnName("binContent").IsRequired();

        builder.Property(x => x.InsertedUserId).HasColumnName("iInsertedUserId");
        builder.Property(x => x.InsertedTime).HasColumnName("dtInsertedTime").HasColumnType("timestamp(3)");
        builder.Property(x => x.UpdatedUserId).HasColumnName("iUpdatedUserId");
        builder.Property(x => x.UpdatedTime).HasColumnName("dtUpdatedTime").HasColumnType("timestamp(3)");
    }
}
