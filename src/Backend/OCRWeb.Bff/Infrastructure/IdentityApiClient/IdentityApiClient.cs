namespace OCRWeb.Bff.Infrastructure.IdentityApiClient
{
    public class IdentityApiClient(HttpClient httpClient) : IIdentityApiClient
    {
        private sealed record ProvisionRequestBody(string ProviderKey, string Email, bool EmailVerified, string? DisplayName);
        private sealed record IsActiveResult(bool IsActive);

        public async Task<GoogleProvisionResult> ProvisionGoogleUserAsync(string providerKey, string email, bool emailVerified, string? displayName, CancellationToken ct = default)
        {
            var response = await httpClient.PostAsJsonAsync(
                "/internal/identity/google-provision",
                new ProvisionRequestBody(providerKey, email, emailVerified, displayName),
                ct);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GoogleProvisionResult>(ct);
            return result ?? throw new InvalidOperationException("google-provision returned an empty response body.");
        }

        public async Task<bool> IsUserActiveAsync(int userId, CancellationToken ct = default)
        {
            var response = await httpClient.GetAsync($"/internal/identity/users/{userId}/is-active", ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<IsActiveResult>(ct);
            return result?.IsActive ?? false;
        }
    }
}
