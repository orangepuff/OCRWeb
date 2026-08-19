using OrangepuffPortal.Bff.Endpoints.AuthEndpoints;
using OrangepuffPortal.Bff.Endpoints.AvatarAdminEndpoints;
using OrangepuffPortal.Bff.Endpoints.AvatarEndpoints;
using OrangepuffPortal.Bff.Endpoints.ConfigAdminEndpoints;
using OrangepuffPortal.Bff.Endpoints.ConfigDataAdminEndpoints;
using OrangepuffPortal.Bff.Endpoints.ConfigEndpoints;
using OrangepuffPortal.Bff.Endpoints.ConfigTextAdminEndpoints;
using OrangepuffPortal.Bff.Endpoints.ConfigTextEndpoints;
using OrangepuffPortal.Bff.Endpoints.SecurityRuleCategoryAdminEndpoints;
using OrangepuffPortal.Bff.Endpoints.SecurityRuleItemAdminEndpoints;
using OrangepuffPortal.Bff.Endpoints.ThemeAdminEndpoints;
using OrangepuffPortal.Bff.Endpoints.ThemeUserEndpoints;
using OrangepuffPortal.Bff.Endpoints.UserAdminEndpoints;
using OrangepuffPortal.Bff.Infrastructure;

namespace OrangepuffPortal.Bff
{
    /// <summary>
    /// Maps Portal's Bff-owned routes: /bff/login, /bff/logout, /bff/me(/permissions), /bff/me/avatar, /bff/users/{id}/avatar, /bff/users/{id}/config, /bff/config-text, and the AdminOnly-gated /bff/admin/* group (including /bff/admin/users/{id}/avatar, /bff/admin/config/sections, /bff/admin/config/items, /bff/admin/config-text).
    /// </summary>
    public static class PortalBffEndpointRouteBuilderExtensions
    {
        public static WebApplication MapPortalBffEndpoints(this WebApplication app)
        {
            app.MapAuthEndpoints();
            app.MapThemeUserEndpoints();
            app.MapAvatarEndpoints();
            app.MapConfigEndpoints();
            app.MapConfigTextEndpoints();

            var adminGroup = app.MapGroup("/bff/admin").RequireAuthorization(PortalBffConstants.AdminOnlyPolicy);
            adminGroup.MapUserAdminEndpoints();
            adminGroup.MapSecurityRuleCategoryAdminEndpoints();
            adminGroup.MapSecurityRuleItemAdminEndpoints();
            adminGroup.MapConfigAdminEndpoints();
            adminGroup.MapConfigTextAdminEndpoints();
            adminGroup.MapConfigDataAdminEndpoints();
            adminGroup.MapAvatarAdminEndpoints();
            adminGroup.MapThemeAdminEndpoints();

            return app;
        }
    }
}
