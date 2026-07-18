namespace OCRWeb.Bff.Infrastructure.IdentityApiClient
{
    /// <summary>
    /// Calls OCRWeb.API's internal Identity endpoints on behalf of the Bff.
    /// </summary>
    public interface IIdentityApiClient
    {
        Task<GoogleProvisionResult> ProvisionGoogleUserAsync(string providerKey, string email, bool emailVerified, string? displayName, CancellationToken ct = default);
        Task<bool> IsUserActiveAsync(int userId, CancellationToken ct = default);
    }
}
