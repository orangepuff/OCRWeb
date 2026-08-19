using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrangepuffPortal.ConfigData.Domain.Entity;

namespace OrangepuffPortal.ConfigData.Infrastructure.Configurations;

/// <summary>
/// Maps <see cref="ConfigDataEntry"/> to [configdata].[ConfigData].
/// DB columns follow the type-prefix convention (s=varchar, i=int, dt=datetime, b=bit).
/// The domain keeps clean names.
/// </summary>
public class ConfigDataConfiguration : IEntityTypeConfiguration<ConfigDataEntry>
{
    public void Configure(EntityTypeBuilder<ConfigDataEntry> builder)
    {
        builder.ToTable("ConfigData");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("iId").ValueGeneratedOnAdd();

        builder.Property(x => x.Key).HasColumnName("sKey").HasColumnType("varchar(100)").IsRequired();
        builder.Property(x => x.Value).HasColumnName("sValue").HasColumnType("varchar(max)");
        builder.Property(x => x.AllowEditByScreen).HasColumnName("bAllowEditByScreen");
        builder.Property(x => x.Description).HasColumnName("sDescription").HasColumnType("varchar(255)");

        builder.Property(x => x.InsertedUserId).HasColumnName("iInsertedUserId");
        builder.Property(x => x.InsertedTime).HasColumnName("dtInsertedTime").HasColumnType("datetime");
        builder.Property(x => x.UpdatedUserId).HasColumnName("iUpdatedUserId");
        builder.Property(x => x.UpdatedTime).HasColumnName("dtUpdatedTime").HasColumnType("datetime");

        builder.HasIndex(x => x.Key).IsUnique().HasDatabaseName("UQ_ConfigData_Key");
    }
}
