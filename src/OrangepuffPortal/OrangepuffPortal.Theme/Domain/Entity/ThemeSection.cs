namespace OrangepuffPortal.Theme.Domain.Entity;

/// <summary>
/// A logical grouping of UI elements within a <see cref="Theme"/> (e.g. Project, Buttons, Typography).
/// Maps to [portal].[ThemeSections].
/// </summary>
public class ThemeSection
{
    public int Id { get; private set; }
    public int ThemeId { get; private set; }
    public string SectionCode { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }
    public int? InsertedUserId { get; private set; }
    public DateTime InsertedTime { get; private set; }
    public int? UpdatedUserId { get; private set; }
    public DateTime? UpdatedTime { get; private set; }

    private ThemeSection() { }

    public ThemeSection(int themeId, string sectionCode, string description, int sortOrder, DateTime utcNow, int? insertedUserId = null)
    {
        ThemeId = themeId;
        SectionCode = sectionCode.Trim();
        Description = description.Trim();
        SortOrder = sortOrder;
        InsertedTime = utcNow;
        InsertedUserId = insertedUserId;
    }

    public void Update(string sectionCode, string description, int sortOrder, DateTime utcNow, int? updatedUserId = null)
    {
        SectionCode = sectionCode.Trim();
        Description = description.Trim();
        SortOrder = sortOrder;
        UpdatedTime = utcNow;
        UpdatedUserId = updatedUserId;
    }
}
