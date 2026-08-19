using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OrangepuffPortal.Identity.Domain.Entity;
using OrangepuffPortal.Identity.Domain.Repositories;

namespace OrangepuffPortal.Identity.Infrastructure.Seeding;

/// <summary>
/// Idempotent seeder: if no users exist, creates the default admin from <see cref="SeedOptions"/>
/// with a hashed password. Invoked by the host after migrations run.
/// </summary>
public class UserDbSeeder(
    IUserRepository users,
    IPasswordHasher<User> passwordHasher,
    IOptions<SeedOptions> options)
{
    private readonly SeedOptions _options = options.Value;

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await users.AnyAsync(ct))
            return;

        if (string.IsNullOrWhiteSpace(_options.AdminPassword))
            throw new InvalidOperationException(
                "Seed:AdminPassword is not configured; cannot create the default admin user.");

        var now = DateTime.UtcNow;
        var admin = new User(_options.AdminUsername, _options.AdminEmail, _options.AdminDisplayName, now);
        admin.SetPasswordHash(passwordHasher.HashPassword(admin, _options.AdminPassword), now);

        await users.AddAsync(admin, ct);
        await users.SaveChangesAsync(ct);
    }
}
