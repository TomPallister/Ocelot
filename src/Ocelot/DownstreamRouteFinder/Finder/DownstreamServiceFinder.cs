using Ocelot.Configuration;

namespace Ocelot.DownstreamRouteFinder.Finder
{
    public class DownstreamServiceFinder: IDownstreamServiceFinder
    {
        public string GetServiceName(string upstreamUrlPath, string upstreamQueryString, string upstreamHttpMethod, string upstreamHost, IInternalConfiguration configuration)
        {
            if (upstreamUrlPath.IndexOf('/', 1) == -1)
            {
                return upstreamUrlPath
                    .Substring(1);
            }

            return upstreamUrlPath
                .Substring(1, upstreamUrlPath.IndexOf('/', 1))
                .TrimEnd('/');
        }
    }
}
