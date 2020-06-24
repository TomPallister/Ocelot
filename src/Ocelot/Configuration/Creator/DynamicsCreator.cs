namespace Ocelot.Configuration.Creator
{
    using Builder;
    using File;
    using System.Collections.Generic;
    using System.Linq;

    public class DynamicsCreator : IDynamicsCreator
    {
        private readonly IRateLimitOptionsCreator _rateLimitOptionsCreator;
        private readonly IVersionCreator _versionCreator;

        public DynamicsCreator(IRateLimitOptionsCreator rateLimitOptionsCreator, IVersionCreator versionCreator)
        {
            _rateLimitOptionsCreator = rateLimitOptionsCreator;
            _versionCreator = versionCreator;
        }

        public List<Route> Create(FileConfiguration fileConfiguration)
        {
            return fileConfiguration.DynamicRoutes
                .Select(dynamic => SetUpDynamicRoute(dynamic, fileConfiguration.GlobalConfiguration))
                .ToList();
        }

        private Route SetUpDynamicRoute(FileDynamicRoute fileDynamicRoute, FileGlobalConfiguration globalConfiguration)
        {
            var rateLimitOption = _rateLimitOptionsCreator
                .Create(fileDynamicRoute.RateLimitRule, globalConfiguration);

            var version = _versionCreator.Create(fileDynamicRoute.DownstreamHttpVersion);

            var downstreamRoute = new DownstreamRouteBuilder()
                .WithEnableRateLimiting(rateLimitOption.EnableRateLimiting)
                .WithRateLimitOptions(rateLimitOption)
                .WithServiceName(fileDynamicRoute.ServiceName)
                .WithDownstreamHttpVersion(version)
                .Build();

            var route = new RouteBuilder()
                .WithDownstreamRoute(downstreamRoute)
                .Build();

            return route;
        }
    }
}
