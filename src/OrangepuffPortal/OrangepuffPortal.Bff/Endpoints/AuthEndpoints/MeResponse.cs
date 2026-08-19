namespace OrangepuffPortal.Bff.Endpoints.AuthEndpoints
{
    public record MeResponse(string UserId, string? Email, string? DisplayName, bool IsAdmin, string CultureCode);
}
