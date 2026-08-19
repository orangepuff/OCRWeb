namespace OrangepuffPortal.Bff.Endpoints.ConfigTextAdminEndpoints
{
    public record AddConfigTextRequest(string SModule, string STextCode, string SCultureCode, string STextType, string SText, string? SNote);
}
