using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrangepuffPortal.Config.Domain.Entity;

namespace OrangepuffPortal.Config.Infrastructure.Configurations;

/// <summary>
/// Maps <see cref="ConfigItem"/> to [config].[Configs].
/// DB columns follow the type-prefix convention (s=varchar/nvarchar, i=int, dt=datetime, bt=bit).
/// The domain keeps clean names.
/// </summary>
public class ConfigItemConfiguration : IEntityTypeConfiguration<ConfigItem>
{
    public void Configure(EntityTypeBuilder<ConfigItem> builder)
    {
        builder.ToTable("Configs");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("iId").ValueGeneratedOnAdd();

        builder.Property(x => x.SectionId).HasColumnName("iSectionId");
        builder.Property(x => x.ConfigCode).HasColumnName("sConfigCode").HasColumnType("varchar(60)").IsRequired();
        builder.Property(x => x.ConfigName).HasColumnName("sConfigName").HasColumnType("nvarchar(255)").IsRequired();
        builder.Property(x => x.TextCode).HasColumnName("sTextCode").HasColumnType("varchar(100)").IsRequired();
        builder.Property(x => x.ConfigType).HasColumnName("iConfigType").HasDefaultValue(0);
        builder.Property(x => x.Show).HasColumnName("btShow").HasDefaultValue(true);
        builder.Property(x => x.AllowUserEdit).HasColumnName("btAllowUserEdit").HasDefaultValue(false);
        builder.Property(x => x.SortOrder).HasColumnName("iSortOrder");
        builder.Property(x => x.DefaultStringValue).HasColumnName("sDefaultValue").HasColumnType("nvarchar(255)");
        builder.Property(x => x.DefaultIntValue).HasColumnName("iDefaultValue");
        builder.Property(x => x.DefaultDecimalValue).HasColumnName("nDefaultValue").HasColumnType("decimal(8,3)");
        builder.Property(x => x.DefaultBoolValue).HasColumnName("btDefaultValue");

        builder.Property(x => x.InsertedUserId).HasColumnName("iInsertedUserId");
        builder.Property(x => x.InsertedTime).HasColumnName("dtInsertedTime").HasColumnType("datetime");
        builder.Property(x => x.UpdatedUserId).HasColumnName("iUpdatedUserId");
        builder.Property(x => x.UpdatedTime).HasColumnName("dtUpdatedTime").HasColumnType("datetime");

        builder.HasIndex(x => x.ConfigCode).IsUnique().HasDatabaseName("UQ_Configs_ConfigCode");

        builder.HasOne<ConfigSection>().WithMany().HasForeignKey(x => x.SectionId).OnDelete(DeleteBehavior.Restrict);
    }
}
