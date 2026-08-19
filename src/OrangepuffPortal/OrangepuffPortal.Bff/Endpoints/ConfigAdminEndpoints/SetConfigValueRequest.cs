using OrangepuffPortal.Config.Contract;

namespace OrangepuffPortal.Bff.Endpoints.ConfigAdminEndpoints
{
    public sealed record SetConfigValueRequest(ConfigValueInput Value);
}
