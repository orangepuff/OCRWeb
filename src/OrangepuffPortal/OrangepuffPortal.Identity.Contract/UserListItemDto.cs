namespace OrangepuffPortal.Identity.Contract;

/// <summary>Lightweight list item — no password hash, safe to return for many users.</summary>
public record UserListItemDto(int Id, string Username, string? Email, string? DisplayName, bool IsActive, bool IsTemplateUser, int? ParentId, bool IsAdmin);
