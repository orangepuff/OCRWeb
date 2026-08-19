namespace OrangepuffPortal.Bff.Endpoints.AuthEndpoints
{
    public record PasswordSignInRequest(string UsernameOrEmail, string Password);
}
