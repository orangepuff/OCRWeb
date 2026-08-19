using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrangepuffPortal.Identity.Domain.Entity;

namespace OrangepuffPortal.Identity.Infrastructure.Configurations;

/// <summary>
/// Maps <see cref="SecurityRuleItem"/> to [identity].[SecurityRuleItems].
/// DB columns follow the type-prefix convention (s=nvarchar, bt=bit, i=int, dt=datetime2).
/// The domain keeps clean names.
/// </summary>
public class SecurityRuleItemConfiguration : IEntityTypeConfiguration<SecurityRuleItem>
{
    public void Configure(EntityTypeBuilder<SecurityRuleItem> builder)
    {
        builder.ToTable("SecurityRuleItems");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("iId").ValueGeneratedOnAdd();

        builder.Property(x => x.CategoryId).HasColumnName("iRuleCategoryId").IsRequired();

        builder.Property(x => x.Code).HasColumnName("sSecurityRuleCode").HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_SecurityRuleItems_Code");

        builder.Property(x => x.Description).HasColumnName("sSecurityRuleDesc").HasMaxLength(255).IsRequired();
        builder.Property(x => x.RuleType).HasColumnName("iRuleType").HasConversion<int>().IsRequired();
        builder.Property(x => x.SortOrder).HasColumnName("iSortOrder");
        builder.Property(x => x.TextCode).HasColumnName("sTextCode").HasMaxLength(90);
        builder.Property(x => x.Hidden).HasColumnName("btHidden").HasDefaultValue(false);

        builder.Property(x => x.InsertedUserId).HasColumnName("iInsertedUserId");
        builder.Property(x => x.InsertedTime).HasColumnName("dtInsertedTime").HasColumnType("datetime2(3)");
        builder.Property(x => x.UpdatedUserId).HasColumnName("iUpdatedUserId");
        builder.Property(x => x.UpdatedTime).HasColumnName("dtUpdatedTime").HasColumnType("datetime2(3)");

        builder.HasOne<SecurityRuleCategory>().WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
    }
}
