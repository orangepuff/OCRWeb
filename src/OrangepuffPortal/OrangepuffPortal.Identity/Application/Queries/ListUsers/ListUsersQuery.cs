using MediatR;
using OrangepuffPortal.Identity.Contract;

namespace OrangepuffPortal.Identity.Application.Queries.ListUsers;

/// <summary>List all users.</summary>
public record ListUsersQuery : IRequest<IReadOnlyList<UserListItemDto>>;
