using MediatR;
using OrangepuffPortal.Identity.Application.Commands.AddSecurityRuleCategory;
using OrangepuffPortal.Identity.Application.Commands.AddSecurityRuleItem;
using OrangepuffPortal.Identity.Application.Commands.AddUser;
using OrangepuffPortal.Identity.Application.Commands.ChangeOwnPassword;
using OrangepuffPortal.Identity.Application.Commands.DeleteSecurityRuleCategory;
using OrangepuffPortal.Identity.Application.Commands.DeleteSecurityRuleItem;
using OrangepuffPortal.Identity.Application.Commands.DeleteUser;
using OrangepuffPortal.Identity.Application.Commands.ProvisionGoogleUser;
using OrangepuffPortal.Identity.Application.Commands.SetUserPassword;
using OrangepuffPortal.Identity.Application.Commands.UpdateDisplayName;
using OrangepuffPortal.Identity.Application.Commands.UpdateSecurityRuleCategory;
using OrangepuffPortal.Identity.Application.Commands.UpdateSecurityRuleItem;
using OrangepuffPortal.Identity.Application.Commands.UpdateUser;
using OrangepuffPortal.Identity.Application.Commands.UpdateUserAvatar;
using OrangepuffPortal.Identity.Application.Commands.VerifyPassword;
using OrangepuffPortal.Identity.Application.Queries.GetEffectivePermissions;
using OrangepuffPortal.Identity.Application.Queries.GetUserAvatar;
using OrangepuffPortal.Identity.Application.Queries.IsUserActive;
using OrangepuffPortal.Identity.Application.Queries.IsUserAdmin;
using OrangepuffPortal.Identity.Application.Queries.ListSecurityRuleCategories;
using OrangepuffPortal.Identity.Application.Queries.ListSecurityRuleItems;
using OrangepuffPortal.Identity.Application.Queries.ListUsers;
using OrangepuffPortal.Identity.Contract;
using OrangepuffPortal.Identity.Domain.Enums;
using OrangepuffPortal.Shared.Translation;

namespace OrangepuffPortal.Bff.Infrastructure.IdentityGateway
{
    /// <summary>
    /// In-process <see cref="IIdentityGateway"/> — dispatches straight to the Identity module's
    /// MediatR handlers (no HTTP hop) and maps its Application-layer results onto this facade's
    /// own DTOs, so the Bff's public surface doesn't leak Identity's internal request/result types.
    /// Also where Application-layer raw codes (e.g. "username_taken") become display text: the
    /// Application layer stays code-only, this gateway is the boundary that resolves them via
    /// <see cref="ITranslation"/> before they reach the client — mirrors how
    /// ConfigCatalogAdminService/ConfigTextAdminService translate inline, just at the layer that's
    /// natural for a MediatR-based module (Identity has a separate Bff-facing DTO to populate;
    /// Config/ConfigText don't).
    /// </summary>
    public class IdentityGateway(IMediator mediator, ITranslation translation) : IIdentityGateway
    {
        private const string ModuleName = "OrangepuffPortal.Identity";

        private Task<string> TranslateAsync(string code, CancellationToken ct) => translation.TranslateAsync(code, ModuleName, ct);

        private async Task<string?> TranslateRejectionAsync(string? reason, CancellationToken ct) =>
            reason is null ? null : await TranslateAsync(reason, ct);

        public async Task<GoogleProvisionResult> ProvisionGoogleUserAsync(string providerKey, string email, bool emailVerified, string? displayName, CancellationToken ct = default)
        {
            var result = await mediator.Send(new ProvisionGoogleUserCommand(providerKey, email, emailVerified, displayName), ct);
            return new GoogleProvisionResult(result.Success, result.UserId, result.CultureCode, await TranslateRejectionAsync(result.RejectionReason, ct));
        }

        public async Task<PasswordSignInResult> VerifyPasswordAsync(string usernameOrEmail, string password, CancellationToken ct = default)
        {
            var result = await mediator.Send(new VerifyPasswordCommand(usernameOrEmail, password), ct);
            return new PasswordSignInResult(result.Success, result.UserId, result.Email, result.DisplayName, result.CultureCode, await TranslateRejectionAsync(result.RejectionReason, ct));
        }

        public async Task<SetUserPasswordResult> SetUserPasswordAsync(int userId, string newPassword, CancellationToken ct = default)
        {
            var result = await mediator.Send(new SetUserPasswordCommand(userId, newPassword), ct);
            return new SetUserPasswordResult(
                result.Success, await TranslateRejectionAsync(result.RejectionReason, ct), result.Success ? await TranslateAsync("password_set", ct) : null);
        }

        public async Task<UpdateDisplayNameResult> UpdateDisplayNameAsync(int userId, string displayName, CancellationToken ct = default)
        {
            var result = await mediator.Send(new UpdateDisplayNameCommand(userId, displayName), ct);
            return new UpdateDisplayNameResult(
                result.Success, await TranslateRejectionAsync(result.RejectionReason, ct), result.Success ? await TranslateAsync("display_name_updated", ct) : null);
        }

        public async Task<ChangeOwnPasswordResult> ChangeOwnPasswordAsync(int userId, string currentPassword, string newPassword, CancellationToken ct = default)
        {
            var result = await mediator.Send(new ChangeOwnPasswordCommand(userId, currentPassword, newPassword), ct);
            return new ChangeOwnPasswordResult(
                result.Success, await TranslateRejectionAsync(result.RejectionReason, ct), result.Success ? await TranslateAsync("password_changed", ct) : null);
        }

        public Task<bool> IsUserActiveAsync(int userId, CancellationToken ct = default) =>
            mediator.Send(new IsUserActiveQuery(userId), ct);

        public Task<bool> IsUserAdminAsync(int userId, CancellationToken ct = default) =>
            mediator.Send(new IsUserAdminQuery(userId), ct);

        public Task<IReadOnlyList<EffectivePermissionDto>> GetEffectivePermissionsAsync(int userId, CancellationToken ct = default) =>
            mediator.Send(new GetEffectivePermissionsQuery(userId), ct);

        public async Task<UpdateUserAvatarResult> UpdateAvatarAsync(int userId, byte[]? image, string? contentType, CancellationToken ct = default)
        {
            var result = await mediator.Send(new UpdateUserAvatarCommand(userId, image, contentType), ct);
            var successCode = image is not null ? "avatar_updated" : "avatar_removed";
            return new UpdateUserAvatarResult(
                result.Success, await TranslateRejectionAsync(result.RejectionReason, ct), result.Success ? await TranslateAsync(successCode, ct) : null);
        }

        public Task<UserAvatarDto?> GetAvatarAsync(int userId, CancellationToken ct = default) =>
            mediator.Send(new GetUserAvatarQuery(userId), ct);

        public Task<IReadOnlyList<UserListItemDto>> ListUsersAsync(CancellationToken ct = default) =>
            mediator.Send(new ListUsersQuery(), ct);

        public Task<IReadOnlyList<SecurityRuleCategoryListItemDto>> ListSecurityRuleCategoriesAsync(CancellationToken ct = default) =>
            mediator.Send(new ListSecurityRuleCategoriesQuery(), ct);

        public Task<IReadOnlyList<SecurityRuleItemListItemDto>> ListSecurityRuleItemsAsync(int? categoryId, CancellationToken ct = default) =>
            mediator.Send(new ListSecurityRuleItemsQuery(categoryId), ct);

        public async Task<AddUserResult> AddUserAsync(string username, string? email, string? displayName, int? templateUserId, string? password, int actorUserId, CancellationToken ct = default)
        {
            var result = await mediator.Send(new AddUserCommand(username, email, displayName, templateUserId, password, actorUserId), ct);
            return new AddUserResult(
                result.Success, result.UserId, await TranslateRejectionAsync(result.RejectionReason, ct), result.Success ? await TranslateAsync("user_created", ct) : null);
        }

        public async Task<UpdateUserResult> UpdateUserAsync(int userId, string? email, string? displayName, bool isActive, bool isTemplateUser, int? parentId, CancellationToken ct = default)
        {
            var result = await mediator.Send(new UpdateUserCommand(userId, email, displayName, isActive, isTemplateUser, parentId), ct);
            return new UpdateUserResult(
                result.Success, await TranslateRejectionAsync(result.RejectionReason, ct), result.Success ? await TranslateAsync("user_updated", ct) : null);
        }

        public async Task<DeleteUserResult> DeleteUserAsync(int userId, CancellationToken ct = default)
        {
            var result = await mediator.Send(new DeleteUserCommand(userId), ct);
            return new DeleteUserResult(
                result.Success, await TranslateRejectionAsync(result.RejectionReason, ct), result.Success ? await TranslateAsync("user_deleted", ct) : null);
        }

        public async Task<AddSecurityRuleCategoryResult> AddSecurityRuleCategoryAsync(string categoryDesc, string? textCode, CancellationToken ct = default)
        {
            var result = await mediator.Send(new AddSecurityRuleCategoryCommand(categoryDesc, textCode), ct);
            return new AddSecurityRuleCategoryResult(
                result.Success, result.CategoryId, await TranslateRejectionAsync(result.RejectionReason, ct), result.Success ? await TranslateAsync("category_created", ct) : null);
        }

        public async Task<UpdateSecurityRuleCategoryResult> UpdateSecurityRuleCategoryAsync(int categoryId, string categoryDesc, string? textCode, bool hidden, CancellationToken ct = default)
        {
            var result = await mediator.Send(new UpdateSecurityRuleCategoryCommand(categoryId, categoryDesc, textCode, hidden), ct);
            return new UpdateSecurityRuleCategoryResult(
                result.Success, await TranslateRejectionAsync(result.RejectionReason, ct), result.Success ? await TranslateAsync("category_updated", ct) : null);
        }

        public async Task<DeleteSecurityRuleCategoryResult> DeleteSecurityRuleCategoryAsync(int categoryId, CancellationToken ct = default)
        {
            var result = await mediator.Send(new DeleteSecurityRuleCategoryCommand(categoryId), ct);
            return new DeleteSecurityRuleCategoryResult(
                result.Success, await TranslateRejectionAsync(result.RejectionReason, ct), result.Success ? await TranslateAsync("category_deleted", ct) : null);
        }

        public async Task<AddSecurityRuleItemResult> AddSecurityRuleItemAsync(int categoryId, string code, string description, int ruleType, string? textCode, int? sortOrder, CancellationToken ct = default)
        {
            var result = await mediator.Send(new AddSecurityRuleItemCommand(categoryId, code, description, (RuleType)ruleType, textCode, sortOrder), ct);
            return new AddSecurityRuleItemResult(
                result.Success, result.RuleItemId, await TranslateRejectionAsync(result.RejectionReason, ct), result.Success ? await TranslateAsync("rule_item_created", ct) : null);
        }

        public async Task<UpdateSecurityRuleItemResult> UpdateSecurityRuleItemAsync(int ruleItemId, int categoryId, string description, int ruleType, string? textCode, int? sortOrder, bool hidden, CancellationToken ct = default)
        {
            var result = await mediator.Send(new UpdateSecurityRuleItemCommand(ruleItemId, categoryId, description, (RuleType)ruleType, textCode, sortOrder, hidden), ct);
            return new UpdateSecurityRuleItemResult(
                result.Success, await TranslateRejectionAsync(result.RejectionReason, ct), result.Success ? await TranslateAsync("rule_item_updated", ct) : null);
        }

        public async Task<DeleteSecurityRuleItemResult> DeleteSecurityRuleItemAsync(int ruleItemId, CancellationToken ct = default)
        {
            var result = await mediator.Send(new DeleteSecurityRuleItemCommand(ruleItemId), ct);
            return new DeleteSecurityRuleItemResult(
                result.Success, await TranslateRejectionAsync(result.RejectionReason, ct), result.Success ? await TranslateAsync("rule_item_deleted", ct) : null);
        }
    }
}
