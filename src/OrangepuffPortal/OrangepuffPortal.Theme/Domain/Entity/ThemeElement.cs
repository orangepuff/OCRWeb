namespace OrangepuffPortal.Theme.Domain.Entity;

/// <summary>
/// A named UI component within a <see cref="ThemeSection"/> (e.g. Card, Panel, Badge within Project).
/// Shown as tabs in the Manage Theme property panel. Maps to [portal].[ThemeElements].
/// </summary>
public class ThemeElement
{
    public int Id { get; private set; }
    public int ThemeSectionId { get; private set; }
    public string ElementCode { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }
    public int? InsertedUserId { get; private set; }
    public DateTime InsertedTime { get; private set; }
    public int? UpdatedUserId { get; private set; }
    public DateTime? UpdatedTime { get; private set; }

    private ThemeElement() { }

    public ThemeElement(int themeSectionId, string elementCode, string description, int sortOrder, DateTime utcNow, int? insertedUserId = null)
    {
        ThemeSectionId = themeSectionId;
        ElementCode = elementCode.Trim();
        Description = description.Trim();
        SortOrder = sortOrder;
        InsertedTime = utcNow;
        InsertedUserId = insertedUserId;
    }

    public void Update(string elementCode, string description, int sortOrder, DateTime utcNow, int? updatedUserId = null)
    {
        ElementCode = elementCode.Trim();
        Description = description.Trim();
        SortOrder = sortOrder;
        UpdatedTime = utcNow;
        UpdatedUserId = updatedUserId;
    }
}
