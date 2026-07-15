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
    public string PasswordHash { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime InsertedTime { get; private set; }
    public DateTime? UpdatedTime { get; private set; }

    private User() { } // EF

    public User(string username, string? email, string? displayName, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required.", nameof(username));

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
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));

        PasswordHash = passwordHash;
        UpdatedTime = utcNow;
    }

    public void Deactivate(DateTime utcNow)
    {
        IsActive = false;
        UpdatedTime = utcNow;
    }
}
