namespace OrangepuffPortal.Bff.Endpoints.UserAdminEndpoints
{
    public record UpdateUserRequest(string? Email, string? DisplayName, bool IsActive, bool IsTemplateUser, int? ParentId);
}
