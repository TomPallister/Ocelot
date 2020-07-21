using Ocelot.Responses;
using Ocelot.Values;

namespace Ocelot.DownstreamRouteFinder.UrlMatcher
{
    public interface IUrlPathToUrlTemplateMatcher
    {
        Response<UrlMatch> Match(string upstreamUrlPath, string upstreamQueryString, UpstreamPathTemplate pathTemplate);
    }
}
