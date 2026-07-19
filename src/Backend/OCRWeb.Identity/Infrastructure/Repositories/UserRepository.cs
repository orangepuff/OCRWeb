using Microsoft.EntityFrameworkCore;
using OCRWeb.Identity.Domain.Entity;
using OCRWeb.Identity.Domain.Repositories;

namespace OCRWeb.Identity.Infrastructure.Repositories;

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

    public Task<bool> HasChildUsersAsync(int parentUserId, CancellationToken ct = default) => db.Users.AnyAsync(x => x.ParentId == parentUserId, ct);

    public async Task AddAsync(User user, CancellationToken ct = default) => await db.Users.AddAsync(user, ct);

    public async Task AddExternalLoginAsync(ExternalLogin externalLogin, CancellationToken ct = default) => await db.ExternalLogins.AddAsync(externalLogin, ct);

    public Task DeleteAsync(User user, CancellationToken ct = default)
    {
        db.Users.Remove(user);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
