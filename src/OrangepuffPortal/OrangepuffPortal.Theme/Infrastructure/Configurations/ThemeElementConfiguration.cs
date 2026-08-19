using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrangepuffPortal.Theme.Domain.Entity;

namespace OrangepuffPortal.Theme.Infrastructure.Configurations;

/// <summary>
/// Maps <see cref="ThemeElement"/> to [portal].[ThemeElements].
/// </summary>
public class ThemeElementConfiguration : IEntityTypeConfiguration<ThemeElement>
{
    public void Configure(EntityTypeBuilder<ThemeElement> builder)
    {
        builder.ToTable("ThemeElements");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("iId").ValueGeneratedOnAdd();

        builder.Property(x => x.ThemeSectionId).HasColumnName("iThemeSectionId").IsRequired();
        builder.Property(x => x.ElementCode).HasColumnName("sElementCode").HasColumnType("varchar(100)").IsRequired();
        builder.Property(x => x.Description).HasColumnName("sDescription").HasColumnType("nvarchar(500)").IsRequired();
        builder.Property(x => x.SortOrder).HasColumnName("iSortOrder").HasDefaultValue(0);

        builder.Property(x => x.InsertedUserId).HasColumnName("iInsertedUserId");
        builder.Property(x => x.InsertedTime).HasColumnName("dtInsertedTime").HasColumnType("datetime2(3)");
        builder.Property(x => x.UpdatedUserId).HasColumnName("iUpdatedUserId");
        builder.Property(x => x.UpdatedTime).HasColumnName("dtUpdatedTime").HasColumnType("datetime2(3)");

        builder.HasIndex(x => new { x.ThemeSectionId, x.ElementCode }).IsUnique().HasDatabaseName("UQ_ThemeElements_SectionId_ElementCode");

        builder.HasOne<ThemeSection>().WithMany().HasForeignKey(x => x.ThemeSectionId).OnDelete(DeleteBehavior.Cascade);
    }
}
