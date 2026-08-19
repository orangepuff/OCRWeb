namespace OrangepuffPortal.Identity.Domain.Entity;

/// <summary>
/// Links a <see cref="User"/> to an account on an external identity provider (e.g. Google).
/// Maps to [identity].[ExternalLogins].
/// </summary>
public class ExternalLogin
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public string Provider { get; private set; } = string.Empty;
    public string ProviderKey { get; private set; } = string.Empty;
    public DateTime InsertedTime { get; private set; }

    private ExternalLogin() { } // EF

    public ExternalLogin(int userId, string provider, string providerKey, DateTime utcNow)
    {
        if (userId <= 0)
        {
            throw new ArgumentException("UserId is required.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(provider))
        {
            throw new ArgumentException("Provider is required.", nameof(provider));
        }

        if (string.IsNullOrWhiteSpace(providerKey))
        {
            throw new ArgumentException("ProviderKey is required.", nameof(providerKey));
        }

        UserId = userId;
        Provider = provider.Trim();
        ProviderKey = providerKey.Trim();
        InsertedTime = utcNow;
    }
}
