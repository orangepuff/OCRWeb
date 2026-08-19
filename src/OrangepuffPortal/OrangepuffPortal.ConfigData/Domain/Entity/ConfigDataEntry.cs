namespace OrangepuffPortal.ConfigData.Domain.Entity;

/// <summary>
/// Application-level configuration value — a named key/value pair that applies globally,
/// independent of any user. Maps to [configdata].[ConfigData].
/// </summary>
/// <remarks>
/// Does not use the shared portal <c>AuditableEntity</c>: the row can be seeded by unattended
/// startup code (no signed-in user), so both audit user-id columns are nullable.
/// </remarks>
public class ConfigDataEntry
{
    public int Id { get; private set; }
    public string Key { get; private set; } = string.Empty;
    public string? Value { get; private set; }
    public int? InsertedUserId { get; private set; }
    public DateTime? InsertedTime { get; private set; }
    public int? UpdatedUserId { get; private set; }
    public DateTime? UpdatedTime { get; private set; }
    public bool? AllowEditByScreen { get; private set; }
    public string? Description { get; private set; }

    private ConfigDataEntry() { } // EF

    public ConfigDataEntry(string key, string? value, bool? allowEditByScreen, string? description, DateTime utcNow, int? insertedUserId = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key is required.", nameof(key));
        }

        Key = key.Trim();
        Value = value;
        AllowEditByScreen = allowEditByScreen;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        InsertedTime = utcNow;
        InsertedUserId = insertedUserId;
    }

    public void AdminUpdate(string key, string? value, bool? allowEditByScreen, string? description, int? updatedUserId, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key is required.", nameof(key));
        }

        Key = key.Trim();
        Value = value;
        AllowEditByScreen = allowEditByScreen;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        UpdatedUserId = updatedUserId;
        UpdatedTime = utcNow;
    }
}
