using Ocelot.DownstreamRouteFinder.UrlMatcher;
using Ocelot.Values;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ocelot.DownstreamRouteFinder.HeaderMatcher
{
    public interface IHeaderPlaceholderNameAndValueFinder
    {
        List<PlaceholderNameAndValue> Find(Dictionary<string, string> upstreamHeaders, Dictionary<string, UpstreamHeaderTemplate> templateHeaders);
    }
}
