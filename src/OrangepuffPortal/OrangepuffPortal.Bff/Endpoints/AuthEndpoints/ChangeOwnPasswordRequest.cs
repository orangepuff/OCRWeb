namespace OrangepuffPortal.Bff.Endpoints.AuthEndpoints
{
    public record ChangeOwnPasswordRequest(string CurrentPassword, string NewPassword);
}
