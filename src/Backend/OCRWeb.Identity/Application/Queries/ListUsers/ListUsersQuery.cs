using MediatR;
using OCRWeb.Identity.Contract;

namespace OCRWeb.Identity.Application.Queries.ListUsers;

/// <summary>List all users.</summary>
public record ListUsersQuery : IRequest<IReadOnlyList<UserListItemDto>>;
