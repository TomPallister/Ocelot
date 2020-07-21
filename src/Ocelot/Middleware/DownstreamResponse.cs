using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace Ocelot.Middleware
{
    public class DownstreamResponse
    {
        public DownstreamResponse(HttpContent content, HttpStatusCode statusCode, List<Header> headers, string reasonPhrase)
        {
            Content = content;
            StatusCode = statusCode;
            Headers = headers ?? new List<Header>();
            ReasonPhrase = reasonPhrase;
        }

        public DownstreamResponse(HttpResponseMessage response)
            : this(response.Content, response.StatusCode, response.Headers.Select(x => new Header(x.Key, x.Value)).ToList(), response.ReasonPhrase)
        {
        }

        public DownstreamResponse(HttpContent content, HttpStatusCode statusCode, IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers, string reasonPhrase)
            : this(content, statusCode, headers.Select(x => new Header(x.Key, x.Value)).ToList(), reasonPhrase)
        {
        }

        public HttpContent Content { get; }
        public HttpStatusCode StatusCode { get; }
        public List<Header> Headers { get; }
        public string ReasonPhrase { get; }
    }
}
