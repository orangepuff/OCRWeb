namespace OrangepuffPortal.Theme.Domain.Entity;

/// <summary>
/// A single style property for a <see cref="ThemeElement"/> (e.g. Background Color = #FFFFFF).
/// <see cref="PropertyType"/> drives how the admin UI renders the control:
///   "color"       → colour picker + hex text input
///   "number_unit" → numeric input + unit dropdown (px/rem/%)
///   "dropdown"    → select list (allowed values stored as semicolon-delimited in <see cref="AllowedValues"/>)
///   "text"        → plain text input
/// Maps to [portal].[ThemeDetails].
/// </summary>
public class ThemeDetail
{
    public int Id { get; private set; }
    public int ThemeElementId { get; private set; }
    public string PropertyKey { get; private set; } = string.Empty;
    public string PropertyLabel { get; private set; } = string.Empty;
    public string PropertyDescription { get; private set; } = string.Empty;
    public string PropertyType { get; private set; } = string.Empty;
    public string? AllowedValues { get; private set; }
    public string? PropertyValue { get; private set; }
    public string? Unit { get; private set; }
    public int SortOrder { get; private set; }
    public int? InsertedUserId { get; private set; }
    public DateTime InsertedTime { get; private set; }
    public int? UpdatedUserId { get; private set; }
    public DateTime? UpdatedTime { get; private set; }

    private ThemeDetail() { }

    public ThemeDetail(int themeElementId, string propertyKey, string propertyLabel, string propertyDescription,
        string propertyType, string? allowedValues, string? propertyValue, string? unit, int sortOrder,
        DateTime utcNow, int? insertedUserId = null)
    {
        ThemeElementId = themeElementId;
        PropertyKey = propertyKey.Trim();
        PropertyLabel = propertyLabel.Trim();
        PropertyDescription = propertyDescription.Trim();
        PropertyType = propertyType.Trim();
        AllowedValues = allowedValues;
        PropertyValue = propertyValue;
        Unit = unit;
        SortOrder = sortOrder;
        InsertedTime = utcNow;
        InsertedUserId = insertedUserId;
    }

    public void UpdateValue(string? propertyValue, string? unit, DateTime utcNow, int? updatedUserId = null)
    {
        PropertyValue = propertyValue;
        Unit = unit;
        UpdatedTime = utcNow;
        UpdatedUserId = updatedUserId;
    }

    public void UpdateDefinition(string propertyLabel, string propertyDescription, string propertyType,
        string? allowedValues, int sortOrder, DateTime utcNow, int? updatedUserId = null)
    {
        PropertyLabel = propertyLabel.Trim();
        PropertyDescription = propertyDescription.Trim();
        PropertyType = propertyType.Trim();
        AllowedValues = allowedValues;
        SortOrder = sortOrder;
        UpdatedTime = utcNow;
        UpdatedUserId = updatedUserId;
    }
}
