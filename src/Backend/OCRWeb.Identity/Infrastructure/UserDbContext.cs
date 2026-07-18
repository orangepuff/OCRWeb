using Microsoft.EntityFrameworkCore;
using OCRWeb.Identity.Domain.Entity;
using OCRWeb.Identity.Infrastructure.Configurations;

namespace OCRWeb.Identity.Infrastructure;

/// <summary>
/// EF Core context for the Identity bounded context.
/// Owns the [identity] schema.
/// </summary>
public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
{
    public const string Schema = "identity";

    public DbSet<User> Users => Set<User>();
    public DbSet<ExternalLogin> ExternalLogins => Set<ExternalLogin>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new ExternalLoginConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
