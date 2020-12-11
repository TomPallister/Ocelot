namespace Ocelot.Configuration.File
{
    using System.Collections.Generic;

    public class FileAggregateRoute : IRoute
    {
        public List<string> RouteKeys { get; set; }
        public List<AggregateRouteConfig> RouteKeysConfig { get; set; }
        public string UpstreamPathTemplate { get; set; }
        public string UpstreamHost { get; set; }
        public bool RouteIsCaseSensitive { get; set; }
        public string Aggregator { get; set; }
        public List<string> UpstreamHttpMethod
        { get; private set; } = new List<string>();

        public int Priority { get; set; } = 1;
    }
}
