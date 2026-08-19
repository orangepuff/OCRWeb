namespace OrangepuffPortal.Bff.Endpoints.UserAdminEndpoints
{
    public record AddUserRequest(string Username, string? Email, string? DisplayName, int? TemplateUserId, string? Password);
}
