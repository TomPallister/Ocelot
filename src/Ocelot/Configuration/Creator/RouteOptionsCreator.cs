namespace Ocelot.Configuration.Creator
{
    using Ocelot.Configuration.Builder;
    using Ocelot.Configuration.File;

    public class RouteOptionsCreator : IRouteOptionsCreator
    {
        public RouteOptions Create(FileRoute fileRoute)
        {
            var isAuthenticated = IsAuthenticated(fileRoute);
            var isAuthorised = IsAuthorised(fileRoute);
            var isCached = IsCached(fileRoute);
            var enableRateLimiting = IsEnableRateLimiting(fileRoute);
            var useServiceDiscovery = !string.IsNullOrEmpty(fileRoute.ServiceName);

            var options = new RouteOptionsBuilder()
                .WithIsAuthenticated(isAuthenticated)
                .WithIsAuthorised(isAuthorised)
                .WithIsCached(isCached)
                .WithRateLimiting(enableRateLimiting)
                .WithUseServiceDiscovery(useServiceDiscovery)
                .Build();

            return options;
        }

        private static bool IsEnableRateLimiting(FileRoute fileRoute)
        {
            return (fileRoute.RateLimitOptions != null && fileRoute.RateLimitOptions.EnableRateLimiting) ? true : false;
        }

        private bool IsAuthenticated(FileRoute fileRoute)
        {
            return !string.IsNullOrEmpty(fileRoute.AuthenticationOptions?.AuthenticationProviderKey);
        }

        private bool IsAuthorised(FileRoute fileRoute)
        {
            return fileRoute.RouteClaimsRequirement?.Count > 0;
        }

        private bool IsCached(FileRoute fileRoute)
        {
            return fileRoute.FileCacheOptions.TtlSeconds > 0;
        }
    }
}
