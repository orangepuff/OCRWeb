using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OCRWeb.Identity.Domain.Entity;

namespace OCRWeb.Identity.Infrastructure.Configurations;

/// <summary>
/// Maps <see cref="User"/> to [identity].[Users].
/// DB columns follow the type-prefix convention (s=nvarchar, bt=bit, i=int, dt=datetime2).
/// The domain keeps clean names.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("iId").ValueGeneratedOnAdd();

        builder.Property(x => x.Username).HasColumnName("sUsername").HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.Username).IsUnique().HasDatabaseName("UQ_Users_Username");

        builder.Property(x => x.Email).HasColumnName("sEmail").HasMaxLength(256);
        builder.Property(x => x.DisplayName).HasColumnName("sDisplayName").HasMaxLength(200);
        builder.Property(x => x.PasswordHash).HasColumnName("sPasswordHash").HasMaxLength(256);
        builder.Property(x => x.IsActive).HasColumnName("btIsActive").HasDefaultValue(true);
        builder.Property(x => x.IsTemplateUser).HasColumnName("btIsTemplateUser").HasDefaultValue(false);
        builder.Property(x => x.ParentId).HasColumnName("iParentId");

        builder.Property(x => x.InsertedTime).HasColumnName("dtInsertedTime").HasColumnType("datetime2(3)");
        builder.Property(x => x.UpdatedTime).HasColumnName("dtUpdatedTime").HasColumnType("datetime2(3)");

        // Self-reference to the template user this user inherits permissions from (one level only).
        builder.HasOne<User>().WithMany().HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
    }
}
