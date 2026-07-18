namespace OCRWeb.API.Endpoints.Internal.GoogleProvision
{
    public record GoogleProvisionResponse(bool Success, int? UserId, string? RejectionReason);
}
