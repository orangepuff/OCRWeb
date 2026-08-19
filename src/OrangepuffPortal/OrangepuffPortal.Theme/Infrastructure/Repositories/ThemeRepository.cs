using Microsoft.EntityFrameworkCore;
using OrangepuffPortal.Theme.Domain.Entity;
using OrangepuffPortal.Theme.Domain.Repositories;

namespace OrangepuffPortal.Theme.Infrastructure.Repositories;

public class ThemeRepository(ThemeDbContext db) : IThemeRepository
{
    public async Task<IReadOnlyList<Domain.Entity.Theme>> ListThemesAsync(CancellationToken cancellationToken = default) =>
        await db.Themes.AsNoTracking().OrderBy(x => x.ThemeCode).ToListAsync(cancellationToken);

    public Task<Domain.Entity.Theme?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        db.Themes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Domain.Entity.Theme?> FindByCodeAsync(string themeCode, CancellationToken cancellationToken = default) =>
        db.Themes.FirstOrDefaultAsync(x => x.ThemeCode == themeCode, cancellationToken);

    public Task<bool> ExistsByCodeAsync(string themeCode, int? excludeId, CancellationToken cancellationToken = default) =>
        db.Themes.AnyAsync(x => x.ThemeCode == themeCode && x.Id != (excludeId ?? -1), cancellationToken);

    public Task<bool> ExistsAsync(int themeId, CancellationToken cancellationToken = default) =>
        db.Themes.AnyAsync(x => x.Id == themeId, cancellationToken);

    public async Task<IReadOnlyList<ThemeSection>> ListSectionsAsync(int themeId, CancellationToken cancellationToken = default) =>
        await db.ThemeSections.AsNoTracking()
            .Where(x => x.ThemeId == themeId)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ThemeElement>> ListElementsAsync(int themeSectionId, CancellationToken cancellationToken = default) =>
        await db.ThemeElements.AsNoTracking()
            .Where(x => x.ThemeSectionId == themeSectionId)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ThemeDetail>> ListDetailsAsync(int themeElementId, CancellationToken cancellationToken = default) =>
        await db.ThemeDetails.AsNoTracking()
            .Where(x => x.ThemeElementId == themeElementId)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

    public async Task<Domain.Entity.Theme?> GetFullThemeAsync(int themeId, CancellationToken cancellationToken = default)
    {
        // Resolve to Default when themeId is 0 (sentinel) or the requested theme doesn't exist.
        Domain.Entity.Theme? theme = themeId > 0
            ? await db.Themes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == themeId, cancellationToken)
            : null;

        if (theme is null)
        {
            theme = await db.Themes.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ThemeCode == Domain.Entity.Theme.DefaultThemeCode, cancellationToken);
        }

        return theme;
    }

    public async Task AddThemeAsync(Domain.Entity.Theme theme, CancellationToken cancellationToken = default) =>
        await db.Themes.AddAsync(theme, cancellationToken);

    public async Task AddSectionAsync(ThemeSection section, CancellationToken cancellationToken = default) =>
        await db.ThemeSections.AddAsync(section, cancellationToken);

    public async Task AddElementAsync(ThemeElement element, CancellationToken cancellationToken = default) =>
        await db.ThemeElements.AddAsync(element, cancellationToken);

    public async Task AddDetailAsync(ThemeDetail detail, CancellationToken cancellationToken = default) =>
        await db.ThemeDetails.AddAsync(detail, cancellationToken);

    public Task DeleteThemeAsync(Domain.Entity.Theme theme, CancellationToken cancellationToken = default)
    {
        db.Themes.Remove(theme);
        return Task.CompletedTask;
    }

    public Task DeleteSectionAsync(ThemeSection section, CancellationToken cancellationToken = default)
    {
        db.ThemeSections.Remove(section);
        return Task.CompletedTask;
    }

    public Task DeleteElementAsync(ThemeElement element, CancellationToken cancellationToken = default)
    {
        db.ThemeElements.Remove(element);
        return Task.CompletedTask;
    }

    public Task DeleteDetailAsync(ThemeDetail detail, CancellationToken cancellationToken = default)
    {
        db.ThemeDetails.Remove(detail);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
