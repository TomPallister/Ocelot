using Ocelot.Configuration;

namespace Ocelot.DownstreamRouteFinder.Finder
{
    public interface IDownstreamServiceFinder
    {
        string GetServiceName(string upstreamUrlPath, string upstreamQueryString, string upstreamHttpMethod, string upstreamHost, IInternalConfiguration configuration);
    }
}
