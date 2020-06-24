using Ocelot.Configuration;
using Ocelot.Responses;

namespace Ocelot.DownstreamRouteFinder.Finder
{
    public interface IDownstreamRouteProvider
    {
        Response<DownstreamRouteHolder> Get(string upstreamUrlPath, string upstreamQueryString, string upstreamHttpMethod, IInternalConfiguration configuration, string upstreamHost);
    }
}
