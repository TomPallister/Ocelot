using Ocelot.Configuration;
using Ocelot.Responses;
using System.Collections.Generic;

namespace Ocelot.DownstreamRouteFinder.Finder
{
    public interface IDownstreamRouteProvider
    {
        Response<DownstreamRouteHolder> Get(string upstreamUrlPath, string upstreamQueryString, string upstreamHttpMethod, IInternalConfiguration configuration, string upstreamHost, Dictionary<string, string> upstreamHeaders);
    }
}
