namespace OrangepuffPortal.Config.Contract.Interfaces;

/// <summary>
/// Eagerly populates <see cref="ICurrentUserConfig"/>'s cache for one user, right after their auth
/// cookie is issued. Called from both Bff login flows (password and Google) so no module pays for a
/// database round trip the first time it reads a config value in a request.
/// </summary>
public interface IUserConfigCacheWarmer
{
    Task WarmAsync(int userId, CancellationToken cancellationToken = default);
}
