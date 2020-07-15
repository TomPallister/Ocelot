using Ocelot.DownstreamRouteFinder.UrlMatcher;
using Ocelot.Responses;
using Ocelot.Values;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ocelot.DownstreamRouteFinder.HeaderMatcher
{
    public interface IHeadersToHeaderTemplatesMatcher
    {
        bool Match(Dictionary<string, string> upstreamHeaders, Dictionary<string, UpstreamHeaderTemplate> routeHeaders);
    }
}
