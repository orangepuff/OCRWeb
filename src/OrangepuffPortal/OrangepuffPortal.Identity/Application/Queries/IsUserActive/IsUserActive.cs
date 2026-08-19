using MediatR;

namespace OrangepuffPortal.Identity.Application.Queries.IsUserActive
{
    /// <summary>
    /// Whether the given user exists and is active.
    /// </summary>
    public record IsUserActiveQuery(int UserId) : IRequest<bool>;
}
