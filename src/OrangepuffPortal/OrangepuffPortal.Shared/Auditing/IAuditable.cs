namespace OrangepuffPortal.Shared.Auditing;

/// <summary>
/// Entities that track who/when created and last modified them.
/// Setters are intentionally not on the interface — use <see cref="Domain.AuditableEntity"/>
/// so audit state can only change through explicit MarkInserted/MarkUpdated calls.
/// </summary>
public interface IAuditable
{
    int InsertedUserId { get; }
    DateTime InsertedTime { get; }
    int? UpdatedUserId { get; }
    DateTime? UpdatedTime { get; }
}
