namespace Ocelot.Configuration.Builder
{
    using Ocelot.Configuration.File;
    using Ocelot.Values;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;

    public class RouteBuilder
    {
        private UpstreamPathTemplate _upstreamTemplatePattern;
        private List<HttpMethod> _upstreamHttpMethod;
        private string _upstreamHost;
        private List<DownstreamRoute> _downstreamRoutes;
        private List<AggregateRouteConfig> _downstreamRoutesConfig;
        private string _aggregator;

        public RouteBuilder()
        {
            _downstreamRoutes = new List<DownstreamRoute>();
            _downstreamRoutesConfig = new List<AggregateRouteConfig>();
        }

        public RouteBuilder WithDownstreamRoute(DownstreamRoute value)
        {
            _downstreamRoutes.Add(value);
            return this;
        }

        public RouteBuilder WithDownstreamRoutes(List<DownstreamRoute> value)
        {
            _downstreamRoutes = value;
            return this;
        }

        public RouteBuilder WithUpstreamHost(string upstreamAddresses)
        {
            _upstreamHost = upstreamAddresses;
            return this;
        }

        public RouteBuilder WithUpstreamPathTemplate(UpstreamPathTemplate input)
        {
            _upstreamTemplatePattern = input;
            return this;
        }

        public RouteBuilder WithUpstreamHttpMethod(List<string> input)
        {
            _upstreamHttpMethod = (input.Count == 0) ? new List<HttpMethod>() : input.Select(x => new HttpMethod(x.Trim())).ToList();
            return this;
        }

        public RouteBuilder WithAggregateRouteConfig(List<AggregateRouteConfig> aggregateRouteConfigs)
        {
            _downstreamRoutesConfig = aggregateRouteConfigs;
            return this;
        }

        public RouteBuilder WithAggregator(string aggregator)
        {
            _aggregator = aggregator;
            return this;
        }

        public Route Build()
        {
            return new Route(
                _downstreamRoutes,
                _downstreamRoutesConfig,
                _upstreamHttpMethod,
                _upstreamTemplatePattern,
                _upstreamHost,
                _aggregator
                );
        }
    }
}
