namespace Ocelot.Configuration
{
    using Ocelot.Configuration.File;
    using Ocelot.Values;
    using System.Collections.Generic;
    using System.Net.Http;

    public class Route
    {
        public Route(List<DownstreamRoute> downstreamRoute,
            List<AggregateRouteConfig> downstreamRouteConfig,
            List<HttpMethod> upstreamHttpMethod,
            UpstreamPathTemplate upstreamTemplatePattern,
            string upstreamHost,
            string aggregator,
            Dictionary<string, UpstreamHeaderTemplate> upstreamHeaderTemplates)
        {
            UpstreamHost = upstreamHost;
            DownstreamRoute = downstreamRoute;
            DownstreamRouteConfig = downstreamRouteConfig;
            UpstreamHttpMethod = upstreamHttpMethod;
            UpstreamTemplatePattern = upstreamTemplatePattern;
            Aggregator = aggregator;
            UpstreamHeaderTemplates = upstreamHeaderTemplates;
        }

        public UpstreamPathTemplate UpstreamTemplatePattern { get; private set; }
        public List<HttpMethod> UpstreamHttpMethod { get; private set; }
        public string UpstreamHost { get; private set; }
        public List<DownstreamRoute> DownstreamRoute { get; private set; }
        public List<AggregateRouteConfig> DownstreamRouteConfig { get; private set; }
        public string Aggregator { get; private set; }
        public Dictionary<string, UpstreamHeaderTemplate> UpstreamHeaderTemplates { get; private set; }
    }
}
