using Ocelot.Errors;

namespace Ocelot.DownstreamRouteFinder.Finder
{
    public class UnableToFindDownstreamRouteError : Error
    {
        public UnableToFindDownstreamRouteError(string path, string httpVerb)
            : base($"Failed to match Route configuration for upstream path: {path}, verb: {httpVerb}.", OcelotErrorCode.UnableToFindDownstreamRouteError, 404)
        {
        }
    }
}
