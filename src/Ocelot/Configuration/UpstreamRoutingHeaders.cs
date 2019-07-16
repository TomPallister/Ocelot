using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Ocelot.Configuration
{
    public class UpstreamRoutingHeaders
    {
        public UpstreamRoutingHeaders(Dictionary<string, HashSet<string>> headers)
        {
            Headers = headers;
        }

        public bool Empty() => Headers.Count == 0;

        public bool HasAnyOf(IHeaderDictionary requestHeaders)
        {
            IHeaderDictionary lowerCaseHeaders = GetLowerCaseHeaders(requestHeaders);
            foreach (KeyValuePair<string, HashSet<string>> h in Headers)
            {
                if (lowerCaseHeaders.TryGetValue(h.Key, out var values))
                {
                    HashSet<string> requestHeaderValues = new HashSet<string>(values);
                    if (h.Value.Overlaps(requestHeaderValues))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool HasAllOf(IHeaderDictionary requestHeaders)
        {
            IHeaderDictionary lowerCaseHeaders = GetLowerCaseHeaders(requestHeaders);
            foreach (KeyValuePair<string, HashSet<string>> h in Headers)
            {
                if (!lowerCaseHeaders.TryGetValue(h.Key, out var values))
                {
                    return false;
                }

                HashSet<string> requestHeaderValues = new HashSet<string>(values);
                if (!h.Value.Overlaps(requestHeaderValues))
                {
                    return false;
                }
            }

            return true;
        }

        private IHeaderDictionary GetLowerCaseHeaders(IHeaderDictionary headers)
        {
            IHeaderDictionary lowerCaseHeaders = new HeaderDictionary();
            foreach (KeyValuePair<string, StringValues> kv in headers)
            {
                string key = kv.Key.ToLowerInvariant();
                StringValues values = new StringValues(kv.Value.Select(v => v.ToLowerInvariant()).ToArray());
                lowerCaseHeaders.Add(key, values);
            }

            return lowerCaseHeaders;
        }

        public Dictionary<string, HashSet<string>> Headers { get; private set; }
    }
}
