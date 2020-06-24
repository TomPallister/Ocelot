namespace Ocelot.DownstreamRouteFinder
{
    using Ocelot.Configuration;
    using Ocelot.DownstreamRouteFinder.UrlMatcher;
    using System.Collections.Generic;

    public class DownstreamRouteHolder
    {
        public DownstreamRouteHolder()
        {
        }

        public DownstreamRouteHolder(List<PlaceholderNameAndValue> templatePlaceholderNameAndValues, Route route)
        {
            TemplatePlaceholderNameAndValues = templatePlaceholderNameAndValues;
            Route = route;
        }

        public List<PlaceholderNameAndValue> TemplatePlaceholderNameAndValues { get; private set; }
        public Route Route { get; private set; }
    }
}
