using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Ocelot.Requester
{
    /// <summary>
    /// This class was made to make unit testing easier when HttpClient is used.
    /// </summary>
    public class HttpClientWrapper : IHttpClient
    {
        public HttpClient Client { get; }

        public HttpClientWrapper(HttpClient client)
        {
            Client = client;
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            return Client.SendAsync(request, cancellationToken);
        }
    }
}
