using Microsoft.EntityFrameworkCore;
using OCRWeb.Shared.Infrastructure.Design;

namespace OCRWeb.Identity.Infrastructure;

/// <summary>Design-time factory for <see cref="UserDbContext"/> (see the shared base).</summary>
public class UserDbContextDesignTimeFactory : DesignTimeDbContextFactoryBase<UserDbContext>
{
    protected override string MigrationsHistorySchema => UserDbContext.Schema;

    protected override UserDbContext Create(DbContextOptions<UserDbContext> options) => new(options);
}
