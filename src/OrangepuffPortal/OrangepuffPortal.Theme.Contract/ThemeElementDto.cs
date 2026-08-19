namespace OrangepuffPortal.Theme.Contract;

/// <summary>
/// A named UI element within a <see cref="ThemeSectionDto"/> (e.g. Card, Panel, Badge within the Project section).
/// Holds the ordered list of style properties that apply to that element.
/// </summary>
public class ThemeElementDto
{
    public int Id { get; init; }
    public string ElementCode { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int SortOrder { get; init; }
    public IReadOnlyList<ThemeDetailDto> Details { get; init; } = [];
}
