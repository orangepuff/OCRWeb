using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrangepuffPortal.Identity.Domain.Entity;

namespace OrangepuffPortal.Identity.Infrastructure.Configurations
{
    /// <summary>
    /// Maps <see cref="ExternalLogin"/> to [identity].[ExternalLogins].
    /// DB columns follow the type-prefix convention (s=nvarchar, i=int, dt=datetime2).
    /// The domain keeps clean names.
    /// </summary>
    public class ExternalLoginConfiguration : IEntityTypeConfiguration<ExternalLogin>
    {
        public void Configure(EntityTypeBuilder<ExternalLogin> builder)
        {
            builder.ToTable("ExternalLogins");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("iId").ValueGeneratedOnAdd();

            builder.Property(x => x.UserId).HasColumnName("iUserId").IsRequired();
            builder.Property(x => x.Provider).HasColumnName("sProvider").HasMaxLength(50).IsRequired();
            builder.Property(x => x.ProviderKey).HasColumnName("sProviderKey").HasMaxLength(256).IsRequired();
            builder.Property(x => x.InsertedTime).HasColumnName("dtInsertedTime").HasColumnType("datetime2(3)");

            builder.HasIndex(x => new { x.Provider, x.ProviderKey }).IsUnique().HasDatabaseName("UQ_ExternalLogins_Provider_ProviderKey");
            builder.HasIndex(x => new { x.UserId, x.Provider }).IsUnique().HasDatabaseName("UQ_ExternalLogins_UserId_Provider");

            builder.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
