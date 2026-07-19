namespace OCRWeb.API.Endpoints.Internal.AddUser
{
    public record AddUserResponse(bool Success, int? UserId, string? RejectionReason);
}
