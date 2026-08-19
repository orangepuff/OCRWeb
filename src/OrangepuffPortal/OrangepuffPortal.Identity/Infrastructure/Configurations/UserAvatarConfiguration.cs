using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrangepuffPortal.Identity.Domain.Entity;

namespace OrangepuffPortal.Identity.Infrastructure.Configurations;

/// <summary>
/// Maps <see cref="UserAvatar"/> to [identity].[UserAvatars].
/// UserId is both PK and FK (1:1 with Users). binAvatar is VARBINARY(MAX).
/// </summary>
public class UserAvatarConfiguration : IEntityTypeConfiguration<UserAvatar>
{
    public void Configure(EntityTypeBuilder<UserAvatar> builder)
    {
        builder.ToTable("UserAvatars");

        builder.HasKey(x => x.UserId);
        builder.Property(x => x.UserId).HasColumnName("iUserId").ValueGeneratedNever();

        builder.Property(x => x.Image).HasColumnName("binAvatar").IsRequired();
        builder.Property(x => x.ContentType).HasColumnName("sContentType").HasMaxLength(100).IsRequired();

        builder.Property(x => x.InsertedTime).HasColumnName("dtInsertedTime").HasColumnType("datetime2(3)");
        builder.Property(x => x.UpdatedTime).HasColumnName("dtUpdatedTime").HasColumnType("datetime2(3)");

        builder.HasOne<User>().WithOne().HasForeignKey<UserAvatar>(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
