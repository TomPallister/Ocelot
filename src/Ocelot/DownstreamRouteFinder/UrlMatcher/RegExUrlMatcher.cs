using Ocelot.Responses;
using Ocelot.Values;

namespace Ocelot.DownstreamRouteFinder.UrlMatcher
{
    public class RegExUrlMatcher : IUrlPathToUrlTemplateMatcher
    {
        public Response<UrlMatch> Match(string upstreamUrlPath, string upstreamQueryString, UpstreamPathTemplate pathTemplate)
        {
            if (!pathTemplate.ContainsQueryString)
            {
                return pathTemplate.Pattern.IsMatch(upstreamUrlPath)
                    ? new OkResponse<UrlMatch>(new UrlMatch(true))
                    : new OkResponse<UrlMatch>(new UrlMatch(false));
            }

            return pathTemplate.Pattern.IsMatch($"{upstreamUrlPath}{upstreamQueryString}")
                ? new OkResponse<UrlMatch>(new UrlMatch(true))
                : new OkResponse<UrlMatch>(new UrlMatch(false));
        }
    }
}
