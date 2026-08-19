using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrangepuffPortal.Theme.Contract;
using OrangepuffPortal.Theme.Contract.Interfaces;
using OrangepuffPortal.Theme.Domain.Repositories;
using OrangepuffPortal.Shared.Auditing;

namespace OrangepuffPortal.Theme.Infrastructure;

/// <summary>
/// Resolves a user's theme (falling back to Default when iThemeId = 0 or the theme is missing)
/// and stores the full <see cref="ThemeDto"/> in <see cref="UserThemeCache"/> — called from both
/// login flows immediately after the auth cookie is issued.
/// </summary>
internal sealed class UserThemeCacheWarmer(
    IThemeRepository themeRepository,
    IUserDirectory userDirectory,
    UserThemeCache cache,
    ILogger<UserThemeCacheWarmer> logger) : IUserThemeCacheWarmer
{
    public async Task WarmAsync(int userId, CancellationToken cancellationToken = default)
    {
        const string LogPrefix = nameof(UserThemeCacheWarmer) + "." + nameof(WarmAsync);

        var themeId = await userDirectory.GetUserThemeIdAsync(userId, cancellationToken);
        var theme = await themeRepository.GetFullThemeAsync(themeId, cancellationToken);

        if (theme is null)
        {
            logger.LogWarning("{LogPrefix}: Default theme not found for user {UserId} — theme cache not warmed", LogPrefix, userId);
            return;
        }

        var sections = await themeRepository.ListSectionsAsync(theme.Id, cancellationToken);
        var sectionDtos = new List<ThemeSectionDto>();

        foreach (var section in sections)
        {
            var elements = await themeRepository.ListElementsAsync(section.Id, cancellationToken);
            var elementDtos = new List<ThemeElementDto>();

            foreach (var element in elements)
            {
                var details = await themeRepository.ListDetailsAsync(element.Id, cancellationToken);
                elementDtos.Add(new ThemeElementDto
                {
                    Id = element.Id,
                    ElementCode = element.ElementCode,
                    Description = element.Description,
                    SortOrder = element.SortOrder,
                    Details = details.Select(d => new ThemeDetailDto
                    {
                        Id = d.Id,
                        PropertyKey = d.PropertyKey,
                        PropertyLabel = d.PropertyLabel,
                        PropertyDescription = d.PropertyDescription,
                        PropertyType = d.PropertyType,
                        PropertyValue = d.PropertyValue,
                        Unit = d.Unit,
                        SortOrder = d.SortOrder
                    }).ToList()
                });
            }

            sectionDtos.Add(new ThemeSectionDto
            {
                Id = section.Id,
                SectionCode = section.SectionCode,
                Description = section.Description,
                SortOrder = section.SortOrder,
                Elements = elementDtos
            });
        }

        var dto = new ThemeDto
        {
            Id = theme.Id,
            ThemeCode = theme.ThemeCode,
            Description = theme.Description,
            IsActive = theme.IsActive,
            Sections = sectionDtos
        };

        await cache.SetAsync(userId, dto, cancellationToken);

        logger.LogDebug("{LogPrefix}: warmed theme '{ThemeCode}' for user {UserId}", LogPrefix, dto.ThemeCode, userId);
    }
}
