using Microsoft.EntityFrameworkCore;
using OrangepuffPortal.Shared.Infrastructure.Design;

namespace OrangepuffPortal.Identity.Infrastructure;

/// <summary>
/// Design-time factory for <see cref="UserDbContext"/> (see the shared base).
/// </summary>
public class UserDbContextDesignTimeFactory : DesignTimeDbContextFactoryBase<UserDbContext>
{
    protected override string MigrationsHistorySchema => UserDbContext.Schema;

    protected override UserDbContext Create(DbContextOptions<UserDbContext> options) => new(options);
}
