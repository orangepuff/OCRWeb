using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrangepuffPortal.Theme.Domain.Entity;

namespace OrangepuffPortal.Theme.Infrastructure.Configurations;

/// <summary>
/// Maps <see cref="ThemeDetail"/> to [portal].[ThemeDetails].
/// </summary>
public class ThemeDetailConfiguration : IEntityTypeConfiguration<ThemeDetail>
{
    public void Configure(EntityTypeBuilder<ThemeDetail> builder)
    {
        builder.ToTable("ThemeDetails");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("iId").ValueGeneratedOnAdd();

        builder.Property(x => x.ThemeElementId).HasColumnName("iThemeElementId").IsRequired();
        builder.Property(x => x.PropertyKey).HasColumnName("sPropertyKey").HasColumnType("varchar(100)").IsRequired();
        builder.Property(x => x.PropertyLabel).HasColumnName("sPropertyLabel").HasColumnType("nvarchar(200)").IsRequired();
        builder.Property(x => x.PropertyDescription).HasColumnName("sPropertyDescription").HasColumnType("nvarchar(500)").IsRequired();
        builder.Property(x => x.PropertyType).HasColumnName("sPropertyType").HasColumnType("varchar(50)").IsRequired();
        builder.Property(x => x.AllowedValues).HasColumnName("sAllowedValues").HasColumnType("nvarchar(1000)");
        builder.Property(x => x.PropertyValue).HasColumnName("sPropertyValue").HasColumnType("nvarchar(500)");
        builder.Property(x => x.Unit).HasColumnName("sUnit").HasColumnType("varchar(20)");
        builder.Property(x => x.SortOrder).HasColumnName("iSortOrder").HasDefaultValue(0);

        builder.Property(x => x.InsertedUserId).HasColumnName("iInsertedUserId");
        builder.Property(x => x.InsertedTime).HasColumnName("dtInsertedTime").HasColumnType("datetime2(3)");
        builder.Property(x => x.UpdatedUserId).HasColumnName("iUpdatedUserId");
        builder.Property(x => x.UpdatedTime).HasColumnName("dtUpdatedTime").HasColumnType("datetime2(3)");

        builder.HasIndex(x => new { x.ThemeElementId, x.PropertyKey }).IsUnique().HasDatabaseName("UQ_ThemeDetails_ElementId_PropertyKey");

        builder.HasOne<ThemeElement>().WithMany().HasForeignKey(x => x.ThemeElementId).OnDelete(DeleteBehavior.Cascade);
    }
}
