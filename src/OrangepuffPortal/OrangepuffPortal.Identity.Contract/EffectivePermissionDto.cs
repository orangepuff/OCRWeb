namespace OrangepuffPortal.Identity.Contract;

/// <summary>
/// A user's effective permission for one rule code — already resolved through
/// template-user inheritance (see <c>Users.ParentId</c>), so callers never need to
/// know whether the value came from the user's own assignment or their template's.
/// </summary>
public record EffectivePermissionDto(string Code, string Description, string RuleType, int? Allowed, decimal? AllowedDecimal);
