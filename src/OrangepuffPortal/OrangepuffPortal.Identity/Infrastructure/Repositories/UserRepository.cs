using Microsoft.EntityFrameworkCore;
using OrangepuffPortal.Identity.Domain.Entity;
using OrangepuffPortal.Identity.Domain.Repositories;

namespace OrangepuffPortal.Identity.Infrastructure.Repositories;

public class UserRepository(UserDbContext db) : IUserRepository
{
    public Task<User?> GetByIdAsync(int id, CancellationToken ct = default) => db.Users.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default) => db.Users.FirstOrDefaultAsync(x => x.Username == username, ct);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) => db.Users.FirstOrDefaultAsync(x => x.Email != null && x.Email.ToLower() == email.ToLower(), ct);

    public Task<User?> GetByExternalLoginAsync(string provider, string providerKey, CancellationToken ct = default) =>
        (from u in db.Users
            join el in db.ExternalLogins on u.Id equals el.UserId
            where el.Provider == provider && el.ProviderKey == providerKey
            select u).FirstOrDefaultAsync(ct);

    public Task<bool> AnyAsync(CancellationToken ct = default) => db.Users.AnyAsync(ct);

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default) =>
        await db.Users.OrderBy(x => x.Username).ToListAsync(ct);

    public Task<bool> HasChildUsersAsync(int parentUserId, CancellationToken ct = default) => db.Users.AnyAsync(x => x.ParentId == parentUserId, ct);

    public async Task AddAsync(User user, CancellationToken ct = default) => await db.Users.AddAsync(user, ct);

    public async Task AddExternalLoginAsync(ExternalLogin externalLogin, CancellationToken ct = default) => await db.ExternalLogins.AddAsync(externalLogin, ct);

    public Task DeleteAsync(User user, CancellationToken ct = default)
    {
        db.Users.Remove(user);
        return Task.CompletedTask;
    }

    public Task<UserAvatar?> GetAvatarAsync(int userId, CancellationToken ct = default) =>
        db.UserAvatars.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId, ct);

    public async Task SetAvatarAsync(int userId, byte[]? image, string? contentType, DateTime utcNow, CancellationToken ct = default)
    {
        var existing = await db.UserAvatars.FirstOrDefaultAsync(x => x.UserId == userId, ct);

        if (image is null)
        {
            if (existing is not null)
            {
                db.UserAvatars.Remove(existing);
            }
            return;
        }

        if (existing is null)
        {
            await db.UserAvatars.AddAsync(new UserAvatar(userId, image, contentType ?? "application/octet-stream", utcNow), ct);
        }
        else
        {
            existing.Replace(image, contentType ?? existing.ContentType, utcNow);
        }
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
