using OrangepuffPortal.Identity.Domain.Entity;

namespace OrangepuffPortal.Identity.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByExternalLoginAsync(string provider, string providerKey, CancellationToken ct = default);
    Task<bool> AnyAsync(CancellationToken ct = default);
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default);
    Task<bool> HasChildUsersAsync(int parentUserId, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task AddExternalLoginAsync(ExternalLogin externalLogin, CancellationToken ct = default);
    Task DeleteAsync(User user, CancellationToken ct = default);
    Task<UserAvatar?> GetAvatarAsync(int userId, CancellationToken ct = default);
    Task SetAvatarAsync(int userId, byte[]? image, string? contentType, DateTime utcNow, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
