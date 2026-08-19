namespace OrangepuffPortal.Bff.Infrastructure
{
    /// <summary>
    /// Thrown from OnCreatingTicket when the internal provisioning call rejects a Google sign-in.
    /// Caught by OnRemoteFailure to redirect to Angular's /auth-error page instead of a generic 500.
    /// </summary>
    public class GoogleProvisioningRejectedException(string reason) : Exception(reason)
    {
        public string Reason { get; } = reason;
    }
}
