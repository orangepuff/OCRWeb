namespace OrangepuffPortal.Identity.Domain.Repositories
{
    /// <summary>
    /// Decides whether a new user may be created automatically from an external sign-in.
    /// </summary>
    public interface IUserRegistrationPolicy
    {
        Task<bool> IsSelfRegistrationAllowedAsync(CancellationToken ct = default);
    }
}