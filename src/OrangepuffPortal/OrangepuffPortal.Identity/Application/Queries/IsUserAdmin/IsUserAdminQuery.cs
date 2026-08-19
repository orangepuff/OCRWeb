using MediatR;

namespace OrangepuffPortal.Identity.Application.Queries.IsUserAdmin
{
    /// <summary>
    /// Whether the given user has the coarse-grained super-admin bypass.
    /// </summary>
    public record IsUserAdminQuery(int UserId) : IRequest<bool>;
}
