namespace OCRWeb.API.Endpoints.Internal.AddUser
{
    public class AddUserRequest
    {
        public required string Username { get; set; }
        public string? Email { get; set; }
        public string? DisplayName { get; set; }
        public int? TemplateUserId { get; set; }
    }
}
