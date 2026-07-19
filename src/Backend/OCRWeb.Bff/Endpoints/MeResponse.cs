namespace OCRWeb.Bff.Endpoints
{
    public record MeResponse(string UserId, string? Email, string? DisplayName, bool IsAdmin);
}
