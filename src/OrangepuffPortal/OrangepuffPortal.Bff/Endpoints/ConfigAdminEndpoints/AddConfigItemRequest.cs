namespace OrangepuffPortal.Bff.Endpoints.ConfigAdminEndpoints
{
    public record AddConfigItemRequest(
        int ISectionId, string SConfigCode, string SConfigName, string STextCode, int IConfigType, bool BtShow, bool BtAllowUserEdit, int? ISortOrder,
        string? SDefaultValue, int? IDefaultValue, decimal? NDefaultValue, bool? BtDefaultValue);
}
