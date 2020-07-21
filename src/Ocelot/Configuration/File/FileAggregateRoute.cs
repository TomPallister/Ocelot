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

        // Only supports GET..are you crazy!! POST, PUT WOULD BE CRAZY!! :)
        public List<string> UpstreamHttpMethod
        {
            get { return new List<string> { "Get" }; }
        }

        public int Priority { get; set; } = 1;
    }
}
