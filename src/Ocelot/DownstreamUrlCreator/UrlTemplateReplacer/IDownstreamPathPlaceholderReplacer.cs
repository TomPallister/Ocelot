using Ocelot.DownstreamRouteFinder.UrlMatcher;
using Ocelot.Responses;
using Ocelot.Values;
using System.Collections.Generic;

namespace Ocelot.DownstreamUrlCreator.UrlTemplateReplacer
{
    public interface IDownstreamPathPlaceholderReplacer
    {
        Response<DownstreamPath> Replace(string downstreamPathTemplate, List<PlaceholderNameAndValue> urlPathPlaceholderNameAndValues);
    }
}
