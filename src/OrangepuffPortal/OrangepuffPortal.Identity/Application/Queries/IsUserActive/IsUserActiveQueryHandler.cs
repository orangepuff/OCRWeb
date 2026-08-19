using MediatR;
using Microsoft.Extensions.Logging;
using OrangepuffPortal.Identity.Domain.Repositories;

namespace OrangepuffPortal.Identity.Application.Queries.IsUserActive
{
    public class IsUserActiveQueryHandler(
        IUserRepository repository
        , ILogger<IsUserActiveQueryHandler> logger) : IRequestHandler<IsUserActiveQuery, bool>
    {
        private const string LogPrefix = nameof(IsUserActiveQueryHandler) + "." + nameof(Handle);
        public async Task<bool> Handle(IsUserActiveQuery request, CancellationToken cancellationToken)
        {
            var user = await repository.GetByIdAsync(request.UserId, cancellationToken);

            var isActive = user?.IsActive ?? false;

            if (!isActive)
            {
                logger.LogWarning("{LogPrefix}: user {UserId} is not active — revalidation will sign them out", LogPrefix, request.UserId);
            }

            return isActive;
        }
    }
}