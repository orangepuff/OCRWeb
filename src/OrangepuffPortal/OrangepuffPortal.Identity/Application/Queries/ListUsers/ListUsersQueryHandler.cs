using MediatR;
using OrangepuffPortal.Identity.Contract;
using OrangepuffPortal.Identity.Domain.Repositories;

namespace OrangepuffPortal.Identity.Application.Queries.ListUsers;

public class ListUsersQueryHandler(IUserRepository repository) : IRequestHandler<ListUsersQuery, IReadOnlyList<UserListItemDto>>
{
    public async Task<IReadOnlyList<UserListItemDto>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await repository.GetAllAsync(cancellationToken);
        return users
            .Select(u => new UserListItemDto(u.Id, u.Username, u.Email, u.DisplayName, u.IsActive, u.IsTemplateUser, u.ParentId, u.IsAdmin))
            .ToList();
    }
}
