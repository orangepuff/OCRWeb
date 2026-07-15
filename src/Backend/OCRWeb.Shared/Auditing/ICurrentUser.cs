namespace OCRWeb.Shared.Auditing;

/// <summary>
/// Abstraction over the current authenticated user, used to fill audit fields.
/// Lives in Shared so any module can depend on it without referencing the Identity module.
/// </summary>
public interface ICurrentUser
{
    /// <summary>Id of the current user (matches the INT primary key of [identity].[Users]).</summary>
    int UserId { get; }
}
