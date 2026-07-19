using OCRWeb.Identity.Contract;
using System.Net;
using System.Net.Http.Headers;

namespace OCRWeb.Bff.Infrastructure.IdentityApiClient
{
    public class IdentityApiClient(HttpClient httpClient) : IIdentityApiClient
    {
        private sealed record ProvisionRequestBody(string ProviderKey, string Email, bool EmailVerified, string? DisplayName);
        private sealed record IsActiveResult(bool IsActive);
        private sealed record IsAdminResult(bool IsAdmin);

        private sealed record AddUserRequestBody(string Username, string? Email, string? DisplayName, int? TemplateUserId);
        private sealed record UpdateUserRequestBody(string? Email, string? DisplayName, bool IsTemplateUser, int? ParentId);

        private sealed record AddSecurityRuleCategoryRequestBody(string CategoryDesc, string? TextCode);
        private sealed record UpdateSecurityRuleCategoryRequestBody(string CategoryDesc, string? TextCode, bool Hidden);

        private sealed record AddSecurityRuleItemRequestBody(int CategoryId, string Code, string Description, int RuleType, string? TextCode, int? SortOrder);
        private sealed record UpdateSecurityRuleItemRequestBody(int CategoryId, string Description, int RuleType, string? TextCode, int? SortOrder, bool Hidden);

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

        public async Task<bool> IsUserAdminAsync(int userId, CancellationToken ct = default)
        {
            var response = await httpClient.GetAsync($"/internal/identity/users/{userId}/is-admin", ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<IsAdminResult>(ct);
            return result?.IsAdmin ?? false;
        }

        public async Task<IReadOnlyList<EffectivePermissionDto>> GetEffectivePermissionsAsync(int userId, CancellationToken ct = default)
        {
            var result = await httpClient.GetFromJsonAsync<IReadOnlyList<EffectivePermissionDto>>($"/internal/identity/users/{userId}/permissions", ct);
            return result ?? [];
        }

        public async Task<UpdateUserAvatarResult> UpdateAvatarAsync(int userId, byte[]? image, string? contentType, CancellationToken ct = default)
        {
            using var content = new MultipartFormDataContent();
            if (image is not null)
            {
                var fileContent = new ByteArrayContent(image);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType ?? "application/octet-stream");
                content.Add(fileContent, "File", "avatar");
            }

            var response = await httpClient.PutAsync($"/internal/identity/users/{userId}/avatar", content, ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<UpdateUserAvatarResult>(ct);
            return result ?? throw new InvalidOperationException("update-avatar returned an empty response body.");
        }

        public async Task<UserAvatarDto?> GetAvatarAsync(int userId, CancellationToken ct = default)
        {
            var response = await httpClient.GetAsync($"/internal/identity/users/{userId}/avatar", ct);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            var bytes = await response.Content.ReadAsByteArrayAsync(ct);
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
            return new UserAvatarDto(bytes, contentType);
        }

        public async Task<IReadOnlyList<UserListItemDto>> ListUsersAsync(CancellationToken ct = default)
        {
            var result = await httpClient.GetFromJsonAsync<IReadOnlyList<UserListItemDto>>("/internal/identity/users", ct);
            return result ?? [];
        }

        public async Task<IReadOnlyList<SecurityRuleCategoryListItemDto>> ListSecurityRuleCategoriesAsync(CancellationToken ct = default)
        {
            var result = await httpClient.GetFromJsonAsync<IReadOnlyList<SecurityRuleCategoryListItemDto>>("/internal/identity/security-rule-categories", ct);
            return result ?? [];
        }

        public async Task<IReadOnlyList<SecurityRuleItemListItemDto>> ListSecurityRuleItemsAsync(int? categoryId, CancellationToken ct = default)
        {
            var query = categoryId is int id ? $"?categoryId={id}" : string.Empty;
            var result = await httpClient.GetFromJsonAsync<IReadOnlyList<SecurityRuleItemListItemDto>>($"/internal/identity/security-rule-items{query}", ct);
            return result ?? [];
        }

        public async Task<AddUserResult> AddUserAsync(string username, string? email, string? displayName, int? templateUserId, CancellationToken ct = default)
        {
            var response = await httpClient.PostAsJsonAsync("/internal/identity/users", new AddUserRequestBody(username, email, displayName, templateUserId), ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<AddUserResult>(ct);
            return result ?? throw new InvalidOperationException("add-user returned an empty response body.");
        }

        public async Task<UpdateUserResult> UpdateUserAsync(int userId, string? email, string? displayName, bool isTemplateUser, int? parentId, CancellationToken ct = default)
        {
            var response = await httpClient.PutAsJsonAsync($"/internal/identity/users/{userId}", new UpdateUserRequestBody(email, displayName, isTemplateUser, parentId), ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<UpdateUserResult>(ct);
            return result ?? throw new InvalidOperationException("update-user returned an empty response body.");
        }

        public async Task<DeleteUserResult> DeleteUserAsync(int userId, CancellationToken ct = default)
        {
            var response = await httpClient.DeleteAsync($"/internal/identity/users/{userId}", ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<DeleteUserResult>(ct);
            return result ?? throw new InvalidOperationException("delete-user returned an empty response body.");
        }

        public async Task<AddSecurityRuleCategoryResult> AddSecurityRuleCategoryAsync(string categoryDesc, string? textCode, CancellationToken ct = default)
        {
            var response = await httpClient.PostAsJsonAsync("/internal/identity/security-rule-categories", new AddSecurityRuleCategoryRequestBody(categoryDesc, textCode), ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<AddSecurityRuleCategoryResult>(ct);
            return result ?? throw new InvalidOperationException("add-security-rule-category returned an empty response body.");
        }

        public async Task<UpdateSecurityRuleCategoryResult> UpdateSecurityRuleCategoryAsync(int categoryId, string categoryDesc, string? textCode, bool hidden, CancellationToken ct = default)
        {
            var response = await httpClient.PutAsJsonAsync(
                $"/internal/identity/security-rule-categories/{categoryId}", new UpdateSecurityRuleCategoryRequestBody(categoryDesc, textCode, hidden), ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<UpdateSecurityRuleCategoryResult>(ct);
            return result ?? throw new InvalidOperationException("update-security-rule-category returned an empty response body.");
        }

        public async Task<DeleteSecurityRuleCategoryResult> DeleteSecurityRuleCategoryAsync(int categoryId, CancellationToken ct = default)
        {
            var response = await httpClient.DeleteAsync($"/internal/identity/security-rule-categories/{categoryId}", ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<DeleteSecurityRuleCategoryResult>(ct);
            return result ?? throw new InvalidOperationException("delete-security-rule-category returned an empty response body.");
        }

        public async Task<AddSecurityRuleItemResult> AddSecurityRuleItemAsync(
            int categoryId, string code, string description, int ruleType, string? textCode, int? sortOrder, CancellationToken ct = default)
        {
            var response = await httpClient.PostAsJsonAsync(
                "/internal/identity/security-rule-items", new AddSecurityRuleItemRequestBody(categoryId, code, description, ruleType, textCode, sortOrder), ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<AddSecurityRuleItemResult>(ct);
            return result ?? throw new InvalidOperationException("add-security-rule-item returned an empty response body.");
        }

        public async Task<UpdateSecurityRuleItemResult> UpdateSecurityRuleItemAsync(
            int ruleItemId, int categoryId, string description, int ruleType, string? textCode, int? sortOrder, bool hidden, CancellationToken ct = default)
        {
            var response = await httpClient.PutAsJsonAsync(
                $"/internal/identity/security-rule-items/{ruleItemId}",
                new UpdateSecurityRuleItemRequestBody(categoryId, description, ruleType, textCode, sortOrder, hidden), ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<UpdateSecurityRuleItemResult>(ct);
            return result ?? throw new InvalidOperationException("update-security-rule-item returned an empty response body.");
        }

        public async Task<DeleteSecurityRuleItemResult> DeleteSecurityRuleItemAsync(int ruleItemId, CancellationToken ct = default)
        {
            var response = await httpClient.DeleteAsync($"/internal/identity/security-rule-items/{ruleItemId}", ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<DeleteSecurityRuleItemResult>(ct);
            return result ?? throw new InvalidOperationException("delete-security-rule-item returned an empty response body.");
        }
    }
}
