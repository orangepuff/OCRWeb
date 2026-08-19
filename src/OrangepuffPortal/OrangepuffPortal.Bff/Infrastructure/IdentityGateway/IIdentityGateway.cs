using OrangepuffPortal.Identity.Contract;

namespace OrangepuffPortal.Bff.Infrastructure.IdentityGateway
{
    /// <summary>
    /// Facade the Bff uses to reach the Identity module's application layer, in-process.
    /// </summary>
    public interface IIdentityGateway
    {
        Task<GoogleProvisionResult> ProvisionGoogleUserAsync(string providerKey, string email, bool emailVerified, string? displayName, CancellationToken ct = default);
        Task<PasswordSignInResult> VerifyPasswordAsync(string usernameOrEmail, string password, CancellationToken ct = default);
        Task<SetUserPasswordResult> SetUserPasswordAsync(int userId, string newPassword, CancellationToken ct = default);
        Task<UpdateDisplayNameResult> UpdateDisplayNameAsync(int userId, string displayName, CancellationToken ct = default);
        Task<ChangeOwnPasswordResult> ChangeOwnPasswordAsync(int userId, string currentPassword, string newPassword, CancellationToken ct = default);
        Task<bool> IsUserActiveAsync(int userId, CancellationToken ct = default);
        Task<bool> IsUserAdminAsync(int userId, CancellationToken ct = default);

        Task<IReadOnlyList<EffectivePermissionDto>> GetEffectivePermissionsAsync(int userId, CancellationToken ct = default);

        Task<UpdateUserAvatarResult> UpdateAvatarAsync(int userId, byte[]? image, string? contentType, CancellationToken ct = default);
        Task<UserAvatarDto?> GetAvatarAsync(int userId, CancellationToken ct = default);

        Task<IReadOnlyList<UserListItemDto>> ListUsersAsync(CancellationToken ct = default);
        Task<IReadOnlyList<SecurityRuleCategoryListItemDto>> ListSecurityRuleCategoriesAsync(CancellationToken ct = default);
        Task<IReadOnlyList<SecurityRuleItemListItemDto>> ListSecurityRuleItemsAsync(int? categoryId, CancellationToken ct = default);

        Task<AddUserResult> AddUserAsync(string username, string? email, string? displayName, int? templateUserId, string? password, int actorUserId, CancellationToken ct = default);
        Task<UpdateUserResult> UpdateUserAsync(int userId, string? email, string? displayName, bool isActive, bool isTemplateUser, int? parentId, CancellationToken ct = default);
        Task<DeleteUserResult> DeleteUserAsync(int userId, CancellationToken ct = default);

        Task<AddSecurityRuleCategoryResult> AddSecurityRuleCategoryAsync(string categoryDesc, string? textCode, CancellationToken ct = default);
        Task<UpdateSecurityRuleCategoryResult> UpdateSecurityRuleCategoryAsync(int categoryId, string categoryDesc, string? textCode, bool hidden, CancellationToken ct = default);
        Task<DeleteSecurityRuleCategoryResult> DeleteSecurityRuleCategoryAsync(int categoryId, CancellationToken ct = default);

        Task<AddSecurityRuleItemResult> AddSecurityRuleItemAsync(int categoryId, string code, string description, int ruleType, string? textCode, int? sortOrder, CancellationToken ct = default);
        Task<UpdateSecurityRuleItemResult> UpdateSecurityRuleItemAsync(int ruleItemId, int categoryId, string description, int ruleType, string? textCode, int? sortOrder, bool hidden, CancellationToken ct = default);
        Task<DeleteSecurityRuleItemResult> DeleteSecurityRuleItemAsync(int ruleItemId, CancellationToken ct = default);
    }
}
