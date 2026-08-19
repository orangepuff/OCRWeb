using MediatR;
using OrangepuffPortal.Identity.Contract;

namespace OrangepuffPortal.Identity.Application.Queries.GetEffectivePermissions;

/// <summary>
/// Resolves a user's effective permission set — if the user has a <c>ParentId</c>
/// (inherits from a template user), reads the template's assignments instead of the
/// user's own (which will be empty by construction).
/// </summary>
public record GetEffectivePermissionsQuery(int UserId) : IRequest<IReadOnlyList<EffectivePermissionDto>>;
