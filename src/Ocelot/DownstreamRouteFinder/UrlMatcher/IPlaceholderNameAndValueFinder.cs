using Ocelot.Responses;
using System.Collections.Generic;

namespace Ocelot.DownstreamRouteFinder.UrlMatcher
{
    public interface IPlaceholderNameAndValueFinder
    {
        Response<List<PlaceholderNameAndValue>> Find(string path, string query, string pathTemplate);
    }
}
