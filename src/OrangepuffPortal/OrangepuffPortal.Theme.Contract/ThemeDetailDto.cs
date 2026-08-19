namespace OrangepuffPortal.Theme.Contract;

/// <summary>
/// One style property on a <see cref="ThemeElementDto"/>.
/// <see cref="SPropertyType"/> drives how consumers render the control:
/// "color" → colour picker; "number_unit" → number + unit selector; "dropdown" → select list; "text" → plain text input.
/// </summary>
public class ThemeDetailDto
{
    public int Id { get; init; }
    public string PropertyKey { get; init; } = string.Empty;
    public string PropertyLabel { get; init; } = string.Empty;
    public string PropertyDescription { get; init; } = string.Empty;
    public string PropertyType { get; init; } = string.Empty;
    public string? PropertyValue { get; init; }
    public string? Unit { get; init; }
    public int SortOrder { get; init; }
}
