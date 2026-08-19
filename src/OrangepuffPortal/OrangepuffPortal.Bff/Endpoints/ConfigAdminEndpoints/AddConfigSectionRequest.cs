namespace OrangepuffPortal.Bff.Endpoints.ConfigAdminEndpoints
{
    public record AddConfigSectionRequest(string SSectionDesc, string STextCode, bool BtShow, int? ISortOrder);
}
