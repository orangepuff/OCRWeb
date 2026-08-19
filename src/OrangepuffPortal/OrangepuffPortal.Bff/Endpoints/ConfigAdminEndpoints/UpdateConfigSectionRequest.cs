namespace OrangepuffPortal.Bff.Endpoints.ConfigAdminEndpoints
{
    public record UpdateConfigSectionRequest(string SSectionDesc, string STextCode, bool BtShow, int? ISortOrder);
}
