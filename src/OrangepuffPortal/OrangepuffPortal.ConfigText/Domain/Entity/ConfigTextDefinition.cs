namespace OrangepuffPortal.ConfigText.Domain.Entity;

/// <summary>
/// One resolved (module, code, culture, type) text value. Maps to [configtext].[ConfigTextDefinition].
/// </summary>
/// <remarks>
/// Does not use the shared portal <c>AuditableEntity</c>: every row is written by unattended startup
/// seeding (<see cref="Infrastructure.ConfigTextWriter"/>), which has no signed-in user to attribute the
/// change to, so both audit user-id columns are nullable here unlike the rest of the portal.
/// </remarks>
public class ConfigTextDefinition
{
    public const string WildcardCulture = "*";

    public int Id { get; private set; }
    public string Module { get; private set; } = string.Empty;
    public string TextCode { get; private set; } = string.Empty;
    public string CultureCode { get; private set; } = string.Empty;
    public string TextType { get; private set; } = string.Empty;
    public string Text { get; private set; } = string.Empty;
    public string? Note { get; private set; }
    public int? InsertedUserId { get; private set; }
    public DateTime? InsertedTime { get; private set; }
    public int? UpdatedUserId { get; private set; }
    public DateTime? UpdatedTime { get; private set; }

    private ConfigTextDefinition() { } // EF

    public ConfigTextDefinition(string module, string textCode, string cultureCode, string textType, string text, string? note, DateTime utcNow, int? insertedUserId = null)
    {
        if (string.IsNullOrWhiteSpace(module))
        {
            throw new ArgumentException("Module is required.", nameof(module));
        }

        if (string.IsNullOrWhiteSpace(textCode))
        {
            throw new ArgumentException("TextCode is required.", nameof(textCode));
        }

        if (string.IsNullOrWhiteSpace(cultureCode))
        {
            throw new ArgumentException("CultureCode is required.", nameof(cultureCode));
        }

        if (string.IsNullOrWhiteSpace(textType))
        {
            throw new ArgumentException("TextType is required.", nameof(textType));
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Text is required.", nameof(text));
        }

        Module = module.Trim();
        TextCode = textCode.Trim();
        CultureCode = cultureCode.Trim();
        TextType = textType.Trim();
        Text = text;
        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        InsertedTime = utcNow;
        InsertedUserId = insertedUserId;
    }

    /// <summary>
    /// Overwrite the text/note of an already-seeded row (only reached when the seed entry sets btReplace).
    /// </summary>
    public void Replace(string text, string? note, DateTime utcNow)
    {
        Text = text;
        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        UpdatedTime = utcNow;
    }

    /// <summary>
    /// Full-field overwrite from the admin CRUD screen — unlike <see cref="Replace"/>, this may also
    /// change the row's identity fields (module/textCode/cultureCode/textType).
    /// </summary>
    public void AdminUpdate(string module, string textCode, string cultureCode, string textType, string text, string? note, int? updatedUserId, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(module))
        {
            throw new ArgumentException("Module is required.", nameof(module));
        }

        if (string.IsNullOrWhiteSpace(textCode))
        {
            throw new ArgumentException("TextCode is required.", nameof(textCode));
        }

        if (string.IsNullOrWhiteSpace(cultureCode))
        {
            throw new ArgumentException("CultureCode is required.", nameof(cultureCode));
        }

        if (string.IsNullOrWhiteSpace(textType))
        {
            throw new ArgumentException("TextType is required.", nameof(textType));
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Text is required.", nameof(text));
        }

        Module = module.Trim();
        TextCode = textCode.Trim();
        CultureCode = cultureCode.Trim();
        TextType = textType.Trim();
        Text = text;
        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        UpdatedUserId = updatedUserId;
        UpdatedTime = utcNow;
    }
}
