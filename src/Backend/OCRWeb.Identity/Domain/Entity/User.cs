namespace OCRWeb.Identity.Domain.Entity;

/// <summary>
/// Aggregate root for an application user. Maps to [identity].[Users].
/// Id is an INT identity so it lines up with the audit user-id columns used across modules.
/// Users carry timestamp audit only (no user-id audit — creation is a system concern).
/// </summary>
public class User
{
    public int Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string? DisplayName { get; private set; }
    public string? PasswordHash { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsTemplateUser { get; private set; }
    public int? ParentId { get; private set; }
    public DateTime InsertedTime { get; private set; }
    public DateTime? UpdatedTime { get; private set; }

    private User() { } // EF

    public User(string username, string? email, string? displayName, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username is required.", nameof(username));
        }
        
        Username = username.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim();
        IsActive = true;
        InsertedTime = utcNow;
    }

    /// <summary>Set (or rotate) the password hash. Hashing itself is an infrastructure concern.</summary>
    public void SetPasswordHash(string passwordHash, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));
        }

        PasswordHash = passwordHash;
        UpdatedTime = utcNow;
    }

    public void Deactivate(DateTime utcNow)
    {
        IsActive = false;
        UpdatedTime = utcNow;
    }

    /// <summary>Mark this user as usable as a permission template for other users.</summary>
    public void MarkAsTemplateUser(DateTime utcNow)
    {
        IsTemplateUser = true;
        UpdatedTime = utcNow;
    }

    /// <summary>
    /// Link this user to a template user it inherits permissions from.
    /// Caller must verify <paramref name="templateUserId"/> refers to a user with <see cref="IsTemplateUser"/> set
    /// and that this user has no rows of its own in SecurityUserRuleItems — this entity cannot see other users.
    /// </summary>
    public void SetParent(int templateUserId, DateTime utcNow)
    {
        if (templateUserId <= 0 || templateUserId == Id)
        {
            throw new ArgumentException("TemplateUserId must reference a different, existing user.", nameof(templateUserId));
        }

        ParentId = templateUserId;
        UpdatedTime = utcNow;
    }
}
