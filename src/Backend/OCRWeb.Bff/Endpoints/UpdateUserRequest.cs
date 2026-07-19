namespace OCRWeb.Bff.Endpoints
{
    public record UpdateUserRequest(string? Email, string? DisplayName, bool IsTemplateUser, int? ParentId);
}
