using OrangepuffPortal.Theme.Domain.Entity;

namespace OrangepuffPortal.Theme.Domain.Repositories;

public interface IThemeRepository
{
    Task<IReadOnlyList<Entity.Theme>> ListThemesAsync(CancellationToken cancellationToken = default);
    Task<Entity.Theme?> FindByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Entity.Theme?> FindByCodeAsync(string themeCode, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(string themeCode, int? excludeId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int themeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ThemeSection>> ListSectionsAsync(int themeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ThemeElement>> ListElementsAsync(int themeSectionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ThemeDetail>> ListDetailsAsync(int themeElementId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the full theme tree (sections → elements → details) for <paramref name="themeId"/>.
    /// Returns the Default theme if <paramref name="themeId"/> is 0 or the theme is not found.
    /// </summary>
    Task<Entity.Theme?> GetFullThemeAsync(int themeId, CancellationToken cancellationToken = default);

    Task AddThemeAsync(Entity.Theme theme, CancellationToken cancellationToken = default);
    Task AddSectionAsync(ThemeSection section, CancellationToken cancellationToken = default);
    Task AddElementAsync(ThemeElement element, CancellationToken cancellationToken = default);
    Task AddDetailAsync(ThemeDetail detail, CancellationToken cancellationToken = default);

    Task DeleteThemeAsync(Entity.Theme theme, CancellationToken cancellationToken = default);
    Task DeleteSectionAsync(ThemeSection section, CancellationToken cancellationToken = default);
    Task DeleteElementAsync(ThemeElement element, CancellationToken cancellationToken = default);
    Task DeleteDetailAsync(ThemeDetail detail, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
