using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrangepuffPortal.Config.Domain.Entity;

namespace OrangepuffPortal.Config.Infrastructure.Configurations;

/// <summary>
/// Maps <see cref="ConfigUser"/> to [config].[ConfigUsers].
/// DB columns follow the type-prefix convention (s=nvarchar, i=int, n=decimal, dt=datetime, bt=bit).
/// The domain keeps clean names.
/// </summary>
public class ConfigUserConfiguration : IEntityTypeConfiguration<ConfigUser>
{
    public void Configure(EntityTypeBuilder<ConfigUser> builder)
    {
        builder.ToTable("ConfigUsers");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("iId").ValueGeneratedOnAdd();

        builder.Property(x => x.UserId).HasColumnName("iUserId");
        builder.Property(x => x.ConfigId).HasColumnName("iConfigId");
        builder.Property(x => x.StringValue).HasColumnName("sConfigValue").HasColumnType("nvarchar(255)");
        builder.Property(x => x.IntValue).HasColumnName("iConfigValue");
        builder.Property(x => x.DecimalValue).HasColumnName("nConfigValue").HasColumnType("decimal(8,3)");
        builder.Property(x => x.BoolValue).HasColumnName("btConfigValue");
        builder.Property(x => x.Active).HasColumnName("btActive").HasDefaultValue(true);

        builder.Property(x => x.InsertedUserId).HasColumnName("iInsertedUserId");
        builder.Property(x => x.InsertedTime).HasColumnName("dtInsertedTime").HasColumnType("datetime");
        builder.Property(x => x.UpdatedUserId).HasColumnName("iUpdatedUserId");
        builder.Property(x => x.UpdatedTime).HasColumnName("dtUpdatedTime").HasColumnType("datetime");

        builder.HasIndex(x => new { x.UserId, x.ConfigId }).IsUnique().HasDatabaseName("UQ_ConfigUsers_User_Config");

        builder.HasOne<ConfigItem>().WithMany().HasForeignKey(x => x.ConfigId).OnDelete(DeleteBehavior.Restrict);
    }
}
