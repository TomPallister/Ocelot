using Ocelot.DownstreamRouteFinder.UrlMatcher;
using Ocelot.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ocelot.DownstreamRouteFinder.HeaderMatcher
{
    public class HeaderPlaceholderNameAndValueFinder : IHeaderPlaceholderNameAndValueFinder
    {
        public Response<UrlMatch> Match(Dictionary<string, string> upstreamHeaders, Dictionary<string, string> routeHeaders)
        {
            throw new NotImplementedException();
        }
    }
}
