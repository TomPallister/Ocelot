namespace Ocelot.Configuration.Builder
{
    public class RouteOptionsBuilder
    {
        private bool _isAuthenticated;
        private bool _isAuthorised;
        private bool _isCached;
        private bool _enableRateLimiting;
        private bool _useServiceDiscovery;

        public RouteOptionsBuilder WithIsCached(bool isCached)
        {
            _isCached = isCached;
            return this;
        }

        public RouteOptionsBuilder WithIsAuthenticated(bool isAuthenticated)
        {
            _isAuthenticated = isAuthenticated;
            return this;
        }

        public RouteOptionsBuilder WithIsAuthorised(bool isAuthorised)
        {
            _isAuthorised = isAuthorised;
            return this;
        }

        public RouteOptionsBuilder WithRateLimiting(bool enableRateLimiting)
        {
            _enableRateLimiting = enableRateLimiting;
            return this;
        }

        public RouteOptionsBuilder WithUseServiceDiscovery(bool useServiceDiscovery)
        {
            _useServiceDiscovery = useServiceDiscovery;
            return this;
        }

        public RouteOptions Build()
        {
            return new RouteOptions(_isAuthenticated, _isAuthorised, _isCached, _enableRateLimiting, _useServiceDiscovery);
        }
    }
}
