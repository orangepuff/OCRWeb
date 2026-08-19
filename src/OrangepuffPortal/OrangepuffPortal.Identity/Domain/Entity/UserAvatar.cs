namespace OrangepuffPortal.Identity.Domain.Entity;

/// <summary>
/// A user's profile picture, split from Users so ordinary user queries never drag the blob.
/// 1:1 with <see cref="User"/> via a shared primary key (UserId is PK + FK).
/// </summary>
public class UserAvatar
{
    public int UserId { get; private set; }
    public byte[] Image { get; private set; } = [];
    public string ContentType { get; private set; } = string.Empty;
    public DateTime InsertedTime { get; private set; }
    public DateTime? UpdatedTime { get; private set; }

    private UserAvatar() { } // EF

    public UserAvatar(int userId, byte[] image, string contentType, DateTime utcNow)
    {
        UserId = userId;
        Image = image ?? throw new ArgumentNullException(nameof(image));
        ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType;
        InsertedTime = utcNow;
    }

    public void Replace(byte[] image, string contentType, DateTime utcNow)
    {
        Image = image ?? throw new ArgumentNullException(nameof(image));
        ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType;
        UpdatedTime = utcNow;
    }
}
