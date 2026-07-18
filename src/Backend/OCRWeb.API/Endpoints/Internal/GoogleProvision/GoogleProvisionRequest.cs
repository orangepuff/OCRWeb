namespace OCRWeb.API.Endpoints.Internal.GoogleProvision
{
    public class GoogleProvisionRequest
    {
        public required string ProviderKey { get; set; }
        public required string Email { get; set; }
        public required bool EmailVerified { get; set; }
        public string? DisplayName { get; set; }
    }
}
