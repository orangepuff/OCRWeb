using System.Net.Http.Headers;

namespace OCRWeb.Bff.Infrastructure.InternalToken
{
    /// <summary>
    /// Attaches a freshly minted RS256 bearer token to every outgoing request on the HttpClient it's registered against.
    /// </summary>
    public class InternalTokenDelegatingHandler(IInternalTokenIssuer tokenIssuer) : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenIssuer.MintToken());
            return base.SendAsync(request, cancellationToken);
        }
    }
}
