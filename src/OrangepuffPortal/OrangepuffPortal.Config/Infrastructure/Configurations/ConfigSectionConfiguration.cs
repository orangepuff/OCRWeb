using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrangepuffPortal.Config.Domain.Entity;

namespace OrangepuffPortal.Config.Infrastructure.Configurations;

/// <summary>
/// Maps <see cref="ConfigSection"/> to [config].[ConfigSections].
/// DB columns follow the type-prefix convention (s=varchar/nvarchar, i=int, dt=datetime, bt=bit).
/// The domain keeps clean names.
/// </summary>
public class ConfigSectionConfiguration : IEntityTypeConfiguration<ConfigSection>
{
    public void Configure(EntityTypeBuilder<ConfigSection> builder)
    {
        builder.ToTable("ConfigSections");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("iId").ValueGeneratedOnAdd();

        builder.Property(x => x.SectionDesc).HasColumnName("sSectionDesc").HasColumnType("nvarchar(255)").IsRequired();
        builder.Property(x => x.TextCode).HasColumnName("sTextCode").HasColumnType("varchar(100)").IsRequired();
        builder.Property(x => x.Show).HasColumnName("btShow").HasDefaultValue(true);
        builder.Property(x => x.SortOrder).HasColumnName("iSortOrder");

        builder.Property(x => x.InsertedUserId).HasColumnName("iInsertedUserId");
        builder.Property(x => x.InsertedTime).HasColumnName("dtInsertedTime").HasColumnType("datetime");
        builder.Property(x => x.UpdatedUserId).HasColumnName("iUpdatedUserId");
        builder.Property(x => x.UpdatedTime).HasColumnName("dtUpdatedTime").HasColumnType("datetime");

        // TextCode alone is now the uniqueness key (Module removed) - a consuming app seeding its own
        // default sections is expected to prefix its own STextCode values (e.g. "ocrProjectManagement.general"),
        // same convention already used for ConfigTextDefinition.sTextCode.
        builder.HasIndex(x => x.TextCode)
            .IsUnique()
            .HasDatabaseName("UQ_ConfigSections_TextCode");
    }
}
