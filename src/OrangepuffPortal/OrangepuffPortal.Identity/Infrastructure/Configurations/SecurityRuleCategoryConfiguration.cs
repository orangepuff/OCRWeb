using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrangepuffPortal.Identity.Domain.Entity;

namespace OrangepuffPortal.Identity.Infrastructure.Configurations;

/// <summary>
/// Maps <see cref="SecurityRuleCategory"/> to [identity].[SecurityRuleCategory].
/// DB columns follow the type-prefix convention (s=nvarchar, bt=bit, i=int, dt=datetime2).
/// The domain keeps clean names.
/// </summary>
public class SecurityRuleCategoryConfiguration : IEntityTypeConfiguration<SecurityRuleCategory>
{
    public void Configure(EntityTypeBuilder<SecurityRuleCategory> builder)
    {
        builder.ToTable("SecurityRuleCategory");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("iId").ValueGeneratedOnAdd();

        builder.Property(x => x.CategoryDesc).HasColumnName("sCategoryDesc").HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.CategoryDesc).IsUnique().HasDatabaseName("UQ_SecurityRuleCategory_CategoryDesc");

        builder.Property(x => x.TextCode).HasColumnName("sTextCode").HasMaxLength(60);
        builder.Property(x => x.Hidden).HasColumnName("btHidden").HasDefaultValue(false);

        builder.Property(x => x.InsertedUserId).HasColumnName("iInsertedUserId");
        builder.Property(x => x.InsertedTime).HasColumnName("dtInsertedTime").HasColumnType("datetime2(3)");
        builder.Property(x => x.UpdatedUserId).HasColumnName("iUpdatedUserId");
        builder.Property(x => x.UpdatedTime).HasColumnName("dtUpdatedTime").HasColumnType("datetime2(3)");
    }
}
