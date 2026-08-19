namespace OrangepuffPortal.Theme.Domain.Entity;

/// <summary>
/// Aggregate root for a visual theme. Maps to [portal].[Themes].
/// Only one theme may be active at a time; the "Default" theme (sThemeCode = "Default")
/// is always present and is used as the fallback when a user has no theme assigned (iThemeId = 0)
/// or their assigned theme is not found.
/// </summary>
public class Theme
{
    public int Id { get; private set; }
    public string ThemeCode { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public int? InsertedUserId { get; private set; }
    public DateTime InsertedTime { get; private set; }
    public int? UpdatedUserId { get; private set; }
    public DateTime? UpdatedTime { get; private set; }

    public const string DefaultThemeCode = "Default";

    private Theme() { }

    public Theme(string themeCode, string description, bool isActive, DateTime utcNow, int? insertedUserId = null)
    {
        if (string.IsNullOrWhiteSpace(themeCode))
        {
            throw new ArgumentException("ThemeCode is required.", nameof(themeCode));
        }

        ThemeCode = themeCode.Trim();
        Description = description.Trim();
        IsActive = isActive;
        InsertedTime = utcNow;
        InsertedUserId = insertedUserId;
    }

    public void Update(string themeCode, string description, bool isActive, DateTime utcNow, int? updatedUserId = null)
    {
        ThemeCode = themeCode.Trim();
        Description = description.Trim();
        IsActive = isActive;
        UpdatedTime = utcNow;
        UpdatedUserId = updatedUserId;
    }
}
