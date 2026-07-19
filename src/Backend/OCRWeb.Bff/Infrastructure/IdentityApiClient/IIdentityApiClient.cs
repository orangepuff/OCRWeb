using OCRWeb.Identity.Contract;

namespace OCRWeb.Bff.Infrastructure.IdentityApiClient
{
    /// <summary>
    /// Calls OCRWeb.API's internal Identity endpoints on behalf of the Bff.
    /// </summary>
    public interface IIdentityApiClient
    {
        Task<GoogleProvisionResult> ProvisionGoogleUserAsync(string providerKey, string email, bool emailVerified, string? displayName, CancellationToken ct = default);
        Task<bool> IsUserActiveAsync(int userId, CancellationToken ct = default);
        Task<bool> IsUserAdminAsync(int userId, CancellationToken ct = default);

        Task<IReadOnlyList<EffectivePermissionDto>> GetEffectivePermissionsAsync(int userId, CancellationToken ct = default);

        Task<UpdateUserAvatarResult> UpdateAvatarAsync(int userId, byte[]? image, string? contentType, CancellationToken ct = default);
        Task<UserAvatarDto?> GetAvatarAsync(int userId, CancellationToken ct = default);

        Task<IReadOnlyList<UserListItemDto>> ListUsersAsync(CancellationToken ct = default);
        Task<IReadOnlyList<SecurityRuleCategoryListItemDto>> ListSecurityRuleCategoriesAsync(CancellationToken ct = default);
        Task<IReadOnlyList<SecurityRuleItemListItemDto>> ListSecurityRuleItemsAsync(int? categoryId, CancellationToken ct = default);

        Task<AddUserResult> AddUserAsync(string username, string? email, string? displayName, int? templateUserId, CancellationToken ct = default);
        Task<UpdateUserResult> UpdateUserAsync(int userId, string? email, string? displayName, bool isTemplateUser, int? parentId, CancellationToken ct = default);
        Task<DeleteUserResult> DeleteUserAsync(int userId, CancellationToken ct = default);

        Task<AddSecurityRuleCategoryResult> AddSecurityRuleCategoryAsync(string categoryDesc, string? textCode, CancellationToken ct = default);
        Task<UpdateSecurityRuleCategoryResult> UpdateSecurityRuleCategoryAsync(int categoryId, string categoryDesc, string? textCode, bool hidden, CancellationToken ct = default);
        Task<DeleteSecurityRuleCategoryResult> DeleteSecurityRuleCategoryAsync(int categoryId, CancellationToken ct = default);

        Task<AddSecurityRuleItemResult> AddSecurityRuleItemAsync(int categoryId, string code, string description, int ruleType, string? textCode, int? sortOrder, CancellationToken ct = default);
        Task<UpdateSecurityRuleItemResult> UpdateSecurityRuleItemAsync(int ruleItemId, int categoryId, string description, int ruleType, string? textCode, int? sortOrder, bool hidden, CancellationToken ct = default);
        Task<DeleteSecurityRuleItemResult> DeleteSecurityRuleItemAsync(int ruleItemId, CancellationToken ct = default);
    }
}
