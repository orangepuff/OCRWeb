using OrangepuffPortal.Shared.Auditing;

namespace OrangepuffPortal.Shared.Domain;

/// <summary>
/// Base class for entities carrying the full audit set (user id + timestamp).
/// Audit values are filled explicitly by application handlers (no EF interceptor),
/// via <see cref="MarkInserted"/> / <see cref="MarkUpdated"/>.
/// </summary>
public abstract class AuditableEntity : IAuditable
{
    public int InsertedUserId { get; protected set; }
    public DateTime InsertedTime { get; protected set; }
    public int? UpdatedUserId { get; protected set; }
    public DateTime? UpdatedTime { get; protected set; }

    /// <summary>Stamp creation audit. Call once when the entity is first persisted.</summary>
    public void MarkInserted(int userId, DateTime utcNow)
    {
        InsertedUserId = userId;
        InsertedTime = utcNow;
    }

    /// <summary>Stamp modification audit. Call on every subsequent change.</summary>
    public void MarkUpdated(int userId, DateTime utcNow)
    {
        UpdatedUserId = userId;
        UpdatedTime = utcNow;
    }
}
