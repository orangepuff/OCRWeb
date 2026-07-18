using Microsoft.Extensions.Configuration;
using OCRWeb.Identity.Domain.Repositories;

namespace OCRWeb.Identity.Infrastructure
{
    /// <summary>
    /// Reads Identity:AllowSelfRegistrationViaGoogle from configuration.
    /// A narrow seam so this can move to a feature-flag service (e.g. Flag smith) later without touching callers.
    /// </summary>
    public class ConfigurationUserRegistrationPolicy(IConfiguration configuration) : IUserRegistrationPolicy
    {
        public Task<bool> IsSelfRegistrationAllowedAsync(CancellationToken ct = default) => Task.FromResult(configuration.GetValue("Identity:AllowSelfRegistrationViaGoogle", true));
    }
}
