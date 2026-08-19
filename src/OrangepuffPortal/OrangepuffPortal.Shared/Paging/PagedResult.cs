namespace OrangepuffPortal.Shared.Paging;

/// <summary>
/// One page of <typeparamref name="T"/> plus the total row count across every page (not just this one).
/// </summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount);
