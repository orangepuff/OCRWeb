using Microsoft.EntityFrameworkCore;
using OrangepuffPortal.ConfigData.Domain.Entity;
using OrangepuffPortal.ConfigData.Domain.Repositories;

namespace OrangepuffPortal.ConfigData.Infrastructure.Repositories;

public class ConfigDataRepository(ConfigDataDbContext db) : IConfigDataRepository
{
    public async Task<IReadOnlyList<ConfigDataEntry>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await db.ConfigData.AsNoTracking().OrderBy(x => x.Key).ToListAsync(cancellationToken);

    public Task<ConfigDataEntry?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        db.ConfigData.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<bool> ExistsAsync(string key, int? excludeId, CancellationToken cancellationToken = default) =>
        db.ConfigData.AnyAsync(x => x.Key == key && x.Id != (excludeId ?? -1), cancellationToken);

    public async Task AddAsync(ConfigDataEntry entry, CancellationToken cancellationToken = default) =>
        await db.ConfigData.AddAsync(entry, cancellationToken);

    public Task DeleteAsync(ConfigDataEntry entry, CancellationToken cancellationToken = default)
    {
        db.ConfigData.Remove(entry);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
