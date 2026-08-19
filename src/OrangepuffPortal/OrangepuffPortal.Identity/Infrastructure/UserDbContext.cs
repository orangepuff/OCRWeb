using Microsoft.EntityFrameworkCore;
using OrangepuffPortal.Identity.Domain.Entity;
using OrangepuffPortal.Identity.Infrastructure.Configurations;

namespace OrangepuffPortal.Identity.Infrastructure;

/// <summary>
/// EF Core context for the Identity bounded context.
/// Owns the [identity] schema.
/// </summary>
public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
{
    public const string Schema = "identity";

    public DbSet<User> Users => Set<User>();
    public DbSet<ExternalLogin> ExternalLogins => Set<ExternalLogin>();
    public DbSet<SecurityRuleCategory> SecurityRuleCategories => Set<SecurityRuleCategory>();
    public DbSet<SecurityRuleItem> SecurityRuleItems => Set<SecurityRuleItem>();
    public DbSet<SecurityUserRuleItem> SecurityUserRuleItems => Set<SecurityUserRuleItem>();
    public DbSet<UserAvatar> UserAvatars => Set<UserAvatar>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new ExternalLoginConfiguration());
        modelBuilder.ApplyConfiguration(new SecurityRuleCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new SecurityRuleItemConfiguration());
        modelBuilder.ApplyConfiguration(new SecurityUserRuleItemConfiguration());
        modelBuilder.ApplyConfiguration(new UserAvatarConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
