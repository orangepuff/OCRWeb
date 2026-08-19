using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrangepuffPortal.Config.Domain.Entity;

namespace OrangepuffPortal.Config.Infrastructure.Configurations;

/// <summary>
/// Maps <see cref="ConfigUserHistory"/> to [config].[ConfigUsersHistory].
/// DB columns follow the type-prefix convention (s=nvarchar, i=int, n=decimal, dt=datetime, bt=bit).
/// The domain keeps clean names.
/// </summary>
public class ConfigUserHistoryConfiguration : IEntityTypeConfiguration<ConfigUserHistory>
{
    public void Configure(EntityTypeBuilder<ConfigUserHistory> builder)
    {
        builder.ToTable("ConfigUsersHistory");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("iId").ValueGeneratedOnAdd();

        builder.Property(x => x.ConfigUserId).HasColumnName("iConfigUserId");
        builder.Property(x => x.UserId).HasColumnName("iUserId");
        builder.Property(x => x.ConfigId).HasColumnName("iConfigId");
        builder.Property(x => x.StringValue).HasColumnName("sConfigValue").HasColumnType("nvarchar(255)");
        builder.Property(x => x.IntValue).HasColumnName("iConfigValue");
        builder.Property(x => x.DecimalValue).HasColumnName("nConfigValue").HasColumnType("decimal(8,3)");
        builder.Property(x => x.BoolValue).HasColumnName("btConfigValue");
        builder.Property(x => x.Active).HasColumnName("btActive").HasDefaultValue(true);

        builder.Property(x => x.InsertedUserId).HasColumnName("iInsertedUserId");
        builder.Property(x => x.InsertedTime).HasColumnName("dtInsertedTime").HasColumnType("datetime");

        // Append-only (see ConfigUserHistory's remarks): no CLR property backs these, they stay NULL forever.
        builder.Property<int?>("UpdatedUserId").HasColumnName("iUpdatedUserId");
        builder.Property<DateTime?>("UpdatedTime").HasColumnName("dtUpdatedTime").HasColumnType("datetime");

        builder.HasIndex(x => x.ConfigUserId).HasDatabaseName("IX_ConfigUsersHistory_ConfigUserId");

        builder.HasOne<ConfigUser>().WithMany().HasForeignKey(x => x.ConfigUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
