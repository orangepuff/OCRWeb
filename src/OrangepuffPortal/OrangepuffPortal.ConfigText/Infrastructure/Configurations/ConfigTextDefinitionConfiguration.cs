using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrangepuffPortal.ConfigText.Domain.Entity;

namespace OrangepuffPortal.ConfigText.Infrastructure.Configurations;

/// <summary>
/// Maps <see cref="ConfigTextDefinition"/> to [configtext].[ConfigTextDefinition].
/// DB columns follow the type-prefix convention (s=varchar/nvarchar/nchar, i=int, dt=datetime).
/// The domain keeps clean names.
/// </summary>
public class ConfigTextDefinitionConfiguration : IEntityTypeConfiguration<ConfigTextDefinition>
{
    public void Configure(EntityTypeBuilder<ConfigTextDefinition> builder)
    {
        builder.ToTable("ConfigTextDefinition");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("iId").ValueGeneratedOnAdd();

        builder.Property(x => x.Module).HasColumnName("sModule").HasColumnType("varchar(60)").IsRequired();
        builder.Property(x => x.TextCode).HasColumnName("sTextCode").HasColumnType("varchar(60)").IsRequired();
        builder.Property(x => x.CultureCode).HasColumnName("sCultureCode").HasColumnType("varchar(10)").IsRequired();
        builder.Property(x => x.TextType).HasColumnName("sTextType").HasColumnType("varchar(10)").IsRequired();
        builder.Property(x => x.Text).HasColumnName("sText").HasColumnType("nvarchar(1000)").IsRequired();
        builder.Property(x => x.Note).HasColumnName("sNote").HasColumnType("nchar(255)");

        builder.Property(x => x.InsertedUserId).HasColumnName("iInsertedUserId");
        builder.Property(x => x.InsertedTime).HasColumnName("dtInsertedTime").HasColumnType("datetime");
        builder.Property(x => x.UpdatedUserId).HasColumnName("iUpdatedUserId");
        builder.Property(x => x.UpdatedTime).HasColumnName("dtUpdatedTime").HasColumnType("datetime");

        builder.HasIndex(x => new { x.Module, x.TextCode, x.CultureCode, x.TextType })
            .IsUnique()
            .HasDatabaseName("UQ_ConfigTextDefinition_Module_Code_Culture_Type");
    }
}
