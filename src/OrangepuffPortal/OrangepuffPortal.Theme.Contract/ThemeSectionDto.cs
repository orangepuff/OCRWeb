namespace OrangepuffPortal.Theme.Contract;

/// <summary>
/// A logical grouping of UI elements within a <see cref="ThemeDto"/> (e.g. Project, Buttons, Typography).
/// Shown as items in the Manage Theme sidebar.
/// </summary>
public class ThemeSectionDto
{
    public int Id { get; init; }
    public string SectionCode { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int SortOrder { get; init; }
    public IReadOnlyList<ThemeElementDto> Elements { get; init; } = [];
}
