using Microsoft.EntityFrameworkCore;
using OrangepuffPortal.ConfigText.Domain.Entity;
using OrangepuffPortal.ConfigText.Domain.Repositories;
using OrangepuffPortal.Shared.Paging;

namespace OrangepuffPortal.ConfigText.Infrastructure.Repositories;

public class ConfigTextRepository(ConfigTextDbContext db) : IConfigTextRepository
{
    public Task<ConfigTextDefinition?> FindAsync(string module, string textCode, string cultureCode, string textType, CancellationToken cancellationToken = default) =>
        db.ConfigTextDefinitions.FirstOrDefaultAsync(x =>
            x.Module == module && x.TextCode == textCode && x.CultureCode == cultureCode && x.TextType == textType, cancellationToken);

    public Task<ConfigTextDefinition?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        db.ConfigTextDefinitions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ConfigTextDefinition>> GetForCultureAsync(string cultureCode, CancellationToken cancellationToken = default) =>
        await db.ConfigTextDefinitions
            .AsNoTracking()
            .Where(x => x.CultureCode == cultureCode || x.CultureCode == ConfigTextDefinition.WildcardCulture)
            .ToListAsync(cancellationToken);

    public async Task<PagedResult<ConfigTextDefinition>> ListAsync(
        string? module, string? textCode, string? cultureCode, string? textType, string? textContains, int skip, int take, CancellationToken cancellationToken = default)
    {
        var query = db.ConfigTextDefinitions.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(module))
        {
            query = query.Where(x => x.Module.Contains(module));
        }

        if (!string.IsNullOrWhiteSpace(textCode))
        {
            query = query.Where(x => x.TextCode.Contains(textCode));
        }

        if (!string.IsNullOrWhiteSpace(cultureCode))
        {
            query = query.Where(x => x.CultureCode == cultureCode);
        }

        if (!string.IsNullOrWhiteSpace(textType))
        {
            query = query.Where(x => x.TextType == textType);
        }

        if (!string.IsNullOrWhiteSpace(textContains))
        {
            query = query.Where(x => x.Text.Contains(textContains));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.Module).ThenBy(x => x.TextCode).ThenBy(x => x.CultureCode).ThenBy(x => x.TextType)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new PagedResult<ConfigTextDefinition>(items, totalCount);
    }

    public Task<bool> ExistsAsync(string module, string textCode, string cultureCode, string textType, int? excludeId, CancellationToken cancellationToken = default) =>
        db.ConfigTextDefinitions.AnyAsync(x =>
            x.Module == module && x.TextCode == textCode && x.CultureCode == cultureCode && x.TextType == textType && x.Id != (excludeId ?? -1), cancellationToken);

    public async Task AddAsync(ConfigTextDefinition entry, CancellationToken cancellationToken = default) =>
        await db.ConfigTextDefinitions.AddAsync(entry, cancellationToken);

    public Task DeleteAsync(ConfigTextDefinition entry, CancellationToken cancellationToken = default)
    {
        db.ConfigTextDefinitions.Remove(entry);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => db.SaveChangesAsync(cancellationToken);
}
