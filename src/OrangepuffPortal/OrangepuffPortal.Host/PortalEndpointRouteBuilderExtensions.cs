using OrangepuffPortal.Bff;

namespace OrangepuffPortal.Host
{
    /// <summary>
    /// Umbrella endpoint mapping for everything OrangepuffPortal owns.
    /// </summary>
    public static class PortalEndpointRouteBuilderExtensions
    {
        public static WebApplication MapOrangepuffPortal(this WebApplication app)
        {
            app.MapPortalBffEndpoints();
            return app;
        }
    }
}
