using System.Collections.Generic;
using System.Net;

namespace Ocelot.Cache
{
    public class CachedResponse
    {
        public CachedResponse(
            HttpStatusCode statusCode,
            Dictionary<string, IEnumerable<string>> headers,
            string body,
            Dictionary<string, IEnumerable<string>> contentHeaders,
            string reasonPhrase
            )
        {
            StatusCode = statusCode;
            Headers = headers ?? new Dictionary<string, IEnumerable<string>>();
            ContentHeaders = contentHeaders ?? new Dictionary<string, IEnumerable<string>>();
            Body = body ?? "";
            ReasonPhrase = reasonPhrase;
        }

        public HttpStatusCode StatusCode { get; private set; }

        public Dictionary<string, IEnumerable<string>> Headers { get; private set; }

        public Dictionary<string, IEnumerable<string>> ContentHeaders { get; private set; }

        public string Body { get; private set; }

        public string ReasonPhrase { get; private set; }
    }
}
