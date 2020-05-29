using System;
using System.Linq;
using Ocelot.Configuration;

namespace Ocelot.Cache
{
    using Ocelot.Request.Middleware;
    using System.Text;
    using System.Threading.Tasks;

    public class CacheKeyGenerator : ICacheKeyGenerator
    {
        public string GenerateRequestCacheKey(DownstreamRequest downstreamRequest, DownstreamRoute downstreamRoute)
        {
            string hashedContent = null;
            var downStreamUrlKeyBuilder = new StringBuilder($"{downstreamRequest.Method}-{downstreamRequest.OriginalString}");

            var cacheOptionsHeader = downstreamRoute?.CacheOptions?.Header;

            if (!string.IsNullOrEmpty(cacheOptionsHeader))
            {
                var header = downstreamRequest.Headers.FirstOrDefault(r =>
                        r.Key.Equals(cacheOptionsHeader, StringComparison.OrdinalIgnoreCase))
                    .Value?.FirstOrDefault();

                if (!string.IsNullOrEmpty(header))
                {
                    downStreamUrlKeyBuilder.Append(header);
                }
            }

            if (downstreamRequest.Content != null)
            {
                string requestContentString = Task.Run(async () => await downstreamRequest.Content.ReadAsStringAsync()).Result;
                downStreamUrlKeyBuilder.Append(requestContentString);
            }

            hashedContent = MD5Helper.GenerateMd5(downStreamUrlKeyBuilder.ToString());
            return hashedContent;
        }
    }
}
