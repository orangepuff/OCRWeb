namespace OCRWeb.API.Endpoints.Internal.UpdateUser
{
    public class UpdateUserRequest
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? DisplayName { get; set; }
        public bool IsTemplateUser { get; set; }
        public int? ParentId { get; set; }
    }
}
