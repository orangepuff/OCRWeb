using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrangepuffPortal.Theme.Domain.Entity;

namespace OrangepuffPortal.Theme.Infrastructure.Configurations;

/// <summary>
/// Maps <see cref="ThemeSection"/> to [portal].[ThemeSections].
/// </summary>
public class ThemeSectionConfiguration : IEntityTypeConfiguration<ThemeSection>
{
    public void Configure(EntityTypeBuilder<ThemeSection> builder)
    {
        builder.ToTable("ThemeSections");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("iId").ValueGeneratedOnAdd();

        builder.Property(x => x.ThemeId).HasColumnName("iThemeId").IsRequired();
        builder.Property(x => x.SectionCode).HasColumnName("sSectionCode").HasColumnType("varchar(100)").IsRequired();
        builder.Property(x => x.Description).HasColumnName("sDescription").HasColumnType("nvarchar(500)").IsRequired();
        builder.Property(x => x.SortOrder).HasColumnName("iSortOrder").HasDefaultValue(0);

        builder.Property(x => x.InsertedUserId).HasColumnName("iInsertedUserId");
        builder.Property(x => x.InsertedTime).HasColumnName("dtInsertedTime").HasColumnType("datetime2(3)");
        builder.Property(x => x.UpdatedUserId).HasColumnName("iUpdatedUserId");
        builder.Property(x => x.UpdatedTime).HasColumnName("dtUpdatedTime").HasColumnType("datetime2(3)");

        builder.HasIndex(x => new { x.ThemeId, x.SectionCode }).IsUnique().HasDatabaseName("UQ_ThemeSections_ThemeId_SectionCode");

        builder.HasOne<Domain.Entity.Theme>().WithMany().HasForeignKey(x => x.ThemeId).OnDelete(DeleteBehavior.Cascade);
    }
}
