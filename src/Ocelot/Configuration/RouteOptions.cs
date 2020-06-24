namespace Ocelot.Configuration
{
    public class RouteOptions
    {
        public RouteOptions(bool isAuthenticated, bool isAuthorised, bool isCached, bool isEnableRateLimiting, bool useServiceDiscovery)
        {
            IsAuthenticated = isAuthenticated;
            IsAuthorised = isAuthorised;
            IsCached = isCached;
            EnableRateLimiting = isEnableRateLimiting;
            UseServiceDiscovery = useServiceDiscovery;
        }

        public bool IsAuthenticated { get; private set; }
        public bool IsAuthorised { get; private set; }
        public bool IsCached { get; private set; }
        public bool EnableRateLimiting { get; private set; }
        public bool UseServiceDiscovery { get; private set; }
    }
}
