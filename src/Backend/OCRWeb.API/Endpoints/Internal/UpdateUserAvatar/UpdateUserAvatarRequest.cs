namespace OCRWeb.API.Endpoints.Internal.UpdateUserAvatar
{
    public class UpdateUserAvatarRequest
    {
        public int Id { get; set; }
        public IFormFile? File { get; set; }
    }
}
