using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrangepuffPortal.Identity.Domain.Entity;

namespace OrangepuffPortal.Identity.Infrastructure.Configurations;

/// <summary>
/// Maps <see cref="SecurityUserRuleItem"/> to [identity].[SecurityUserRuleItems].
/// DB columns follow the type-prefix convention (i=int, n=numeric, dt=datetime2).
/// The domain keeps clean names.
/// </summary>
public class SecurityUserRuleItemConfiguration : IEntityTypeConfiguration<SecurityUserRuleItem>
{
    public void Configure(EntityTypeBuilder<SecurityUserRuleItem> builder)
    {
        builder.ToTable("SecurityUserRuleItems");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("iId").ValueGeneratedOnAdd();

        builder.Property(x => x.UserId).HasColumnName("iUserId").IsRequired();
        builder.Property(x => x.RuleItemId).HasColumnName("iRuleItemId").IsRequired();
        builder.Property(x => x.Allowed).HasColumnName("iAllowed");
        builder.Property(x => x.AllowedDecimal).HasColumnName("nAllowed").HasColumnType("numeric(10,2)");

        builder.Property(x => x.InsertedUserId).HasColumnName("iInsertedUserId");
        builder.Property(x => x.InsertedTime).HasColumnName("dtInsertedTime").HasColumnType("datetime2(3)");
        builder.Property(x => x.UpdatedUserId).HasColumnName("iUpdatedUserId");
        builder.Property(x => x.UpdatedTime).HasColumnName("dtUpdatedTime").HasColumnType("datetime2(3)");

        builder.HasIndex(x => new { x.UserId, x.RuleItemId }).IsUnique().HasDatabaseName("UQ_SecurityUserRuleItems_UserId_RuleItemId");

        builder.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<SecurityRuleItem>().WithMany().HasForeignKey(x => x.RuleItemId).OnDelete(DeleteBehavior.Restrict);
    }
}
