namespace OCRWeb.Bff.Infrastructure.InternalToken
{
    /// <summary>
    /// Mints short-lived RS256 tokens OCRWeb.
    /// Bff uses to authenticate its own calls to OCRWeb.API.
    /// </summary>
    public interface IInternalTokenIssuer
    {
        string MintToken(string? subject = null);
    }
}
