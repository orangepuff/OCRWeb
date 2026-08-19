namespace OrangepuffPortal.Theme.Contract;

/// <summary>
/// Full resolved theme — the shape cached per user and returned by <see cref="Interfaces.ICurrentUserTheme"/>.
/// </summary>
public class ThemeDto
{
    public int Id { get; init; }
    public string ThemeCode { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public IReadOnlyList<ThemeSectionDto> Sections { get; init; } = [];
}
