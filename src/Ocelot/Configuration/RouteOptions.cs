namespace Ocelot.Configuration
{
    public class RouteOptions
    {
        public RouteOptions(bool isAuthenticated, bool isAuthorized, bool isCached, bool isEnableRateLimiting, bool useServiceDiscovery)
        {
            IsAuthenticated = isAuthenticated;
            IsAuthorized = isAuthorized;
            IsCached = isCached;
            EnableRateLimiting = isEnableRateLimiting;
            UseServiceDiscovery = useServiceDiscovery;
        }

        public bool IsAuthenticated { get; private set; }
        public bool IsAuthorized { get; private set; }
        public bool IsCached { get; private set; }
        public bool EnableRateLimiting { get; private set; }
        public bool UseServiceDiscovery { get; private set; }
    }
}
