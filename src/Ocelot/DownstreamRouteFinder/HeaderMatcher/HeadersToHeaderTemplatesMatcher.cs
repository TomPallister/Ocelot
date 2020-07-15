using Microsoft.AspNetCore.Routing;
using Ocelot.DownstreamRouteFinder.UrlMatcher;
using Ocelot.Responses;
using Ocelot.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocelot.DownstreamRouteFinder.HeaderMatcher
{
    public class HeadersToHeaderTemplatesMatcher : IHeadersToHeaderTemplatesMatcher
    {
        public bool Match(Dictionary<string, string> upstreamHeaders, Dictionary<string, UpstreamHeaderTemplate> routeHeaders)
        {        
            return routeHeaders == null ||
                upstreamHeaders != null && routeHeaders.All(
                    h => upstreamHeaders.ContainsKey(h.Key) && routeHeaders[h.Key].Pattern.IsMatch(upstreamHeaders[h.Key]));
        }
    }
}
