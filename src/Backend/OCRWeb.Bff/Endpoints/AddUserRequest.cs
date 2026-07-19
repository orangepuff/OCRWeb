namespace OCRWeb.Bff.Endpoints
{
    public record AddUserRequest(string Username, string? Email, string? DisplayName, int? TemplateUserId);
}
