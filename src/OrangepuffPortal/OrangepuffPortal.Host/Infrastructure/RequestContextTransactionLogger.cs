using Diagnostics.Abstractions.Interfaces;
using OrangepuffPortal.Shared.Auditing;

namespace OrangepuffPortal.Host.Infrastructure;

/// <summary>
/// Decorates <see cref="ITransactionLogger"/> so every opened scope is stamped with the current user and request URL automatically.
/// Callers no longer need to call <see cref="ITransactionScope.SetUser"/>/<see cref="ITransactionScope.SetUrl"/> themselves.
/// </summary>
public class RequestContextTransactionLogger(ITransactionLogger inner, ICurrentUser currentUser, IHttpContextAccessor httpContextAccessor) : ITransactionLogger
{
    public ITransactionScope BeginTransaction(string category, string? message = null)
    {
        var scope = inner.BeginTransaction(category, message);

        try
        {
            scope.SetUser(currentUser.UserId.ToString());
        }
        catch (InvalidOperationException)
        {
            // No resolvable user for this request (e.g. the Google provisioning call, which is authenticated but intentionally carries no sub claim yet) — leave sUser unset rather than fail the request just because logging couldn't attribute an actor.
        }

        var request = httpContextAccessor.HttpContext?.Request;
        if (request is not null)
        {
            scope.SetUrl(
                url: $"{request.Path}{request.QueryString}",
                baseUrl: $"{request.Scheme}://{request.Host}");
        }

        return scope;
    }
}
