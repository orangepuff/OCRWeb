using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OrangepuffPortal.Theme.Infrastructure.Configurations;

/// <summary>
/// Maps <see cref="Domain.Entity.Theme"/> to [portal].[Themes].
/// DB columns follow the type-prefix convention (s=nvarchar, bt=bit, i=int, dt=datetime2).
/// </summary>
public class ThemeConfiguration : IEntityTypeConfiguration<Domain.Entity.Theme>
{
    public void Configure(EntityTypeBuilder<Domain.Entity.Theme> builder)
    {
        builder.ToTable("Themes");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("iId").ValueGeneratedOnAdd();

        builder.Property(x => x.ThemeCode).HasColumnName("sThemeCode").HasColumnType("varchar(100)").IsRequired();
        builder.HasIndex(x => x.ThemeCode).IsUnique().HasDatabaseName("UQ_Themes_ThemeCode");

        builder.Property(x => x.Description).HasColumnName("sDescription").HasColumnType("nvarchar(500)").IsRequired();
        builder.Property(x => x.IsActive).HasColumnName("btActive").HasDefaultValue(true);

        builder.Property(x => x.InsertedUserId).HasColumnName("iInsertedUserId");
        builder.Property(x => x.InsertedTime).HasColumnName("dtInsertedTime").HasColumnType("datetime2(3)");
        builder.Property(x => x.UpdatedUserId).HasColumnName("iUpdatedUserId");
        builder.Property(x => x.UpdatedTime).HasColumnName("dtUpdatedTime").HasColumnType("datetime2(3)");
    }
}
