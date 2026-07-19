using MediatR;
using OCRWeb.Identity.Domain.Repositories;

namespace OCRWeb.Identity.Application.Queries.IsUserAdmin
{
    public class IsUserAdminQueryHandler(IUserRepository repository) : IRequestHandler<IsUserAdminQuery, bool>
    {
        public async Task<bool> Handle(IsUserAdminQuery request, CancellationToken cancellationToken)
        {
            var user = await repository.GetByIdAsync(request.UserId, cancellationToken);
            return user?.IsAdmin ?? false;
        }
    }
}
