namespace Ocelot.Configuration.Creator
{
    using Ocelot.Configuration.Builder;
    using Ocelot.Cache;
    using Ocelot.Configuration.File;
    using System.Collections.Generic;
    using System.Linq;

    public class RoutesCreator : IRoutesCreator
    {
        private readonly ILoadBalancerOptionsCreator _loadBalancerOptionsCreator;
        private readonly IClaimsToThingCreator _claimsToThingCreator;
        private readonly IAuthenticationOptionsCreator _authOptionsCreator;
        private readonly IUpstreamTemplatePatternCreator _upstreamTemplatePatternCreator;
        private readonly IRequestIdKeyCreator _requestIdKeyCreator;
        private readonly IQoSOptionsCreator _qosOptionsCreator;
        private readonly IRouteOptionsCreator _fileRouteOptionsCreator;
        private readonly IRateLimitOptionsCreator _rateLimitOptionsCreator;
        private readonly IRegionCreator _regionCreator;
        private readonly IHttpHandlerOptionsCreator _httpHandlerOptionsCreator;
        private readonly IHeaderFindAndReplaceCreator _headerFAndRCreator;
        private readonly IDownstreamAddressesCreator _downstreamAddressesCreator;
        private readonly IRouteKeyCreator _routeKeyCreator;
        private readonly ISecurityOptionsCreator _securityOptionsCreator;
        private readonly IVersionCreator _versionCreator;
        private readonly IConnectionCloseCreator _connectionCloseCreator;

        public RoutesCreator(
            IClaimsToThingCreator claimsToThingCreator,
            IAuthenticationOptionsCreator authOptionsCreator,
            IUpstreamTemplatePatternCreator upstreamTemplatePatternCreator,
            IRequestIdKeyCreator requestIdKeyCreator,
            IQoSOptionsCreator qosOptionsCreator,
            IRouteOptionsCreator fileRouteOptionsCreator,
            IRateLimitOptionsCreator rateLimitOptionsCreator,
            IRegionCreator regionCreator,
            IHttpHandlerOptionsCreator httpHandlerOptionsCreator,
            IHeaderFindAndReplaceCreator headerFAndRCreator,
            IDownstreamAddressesCreator downstreamAddressesCreator,
            ILoadBalancerOptionsCreator loadBalancerOptionsCreator,
            IRouteKeyCreator routeKeyCreator,
            ISecurityOptionsCreator securityOptionsCreator,
            IVersionCreator versionCreator,
            IConnectionCloseCreator connectionCloseCreator
            )
        {
            _routeKeyCreator = routeKeyCreator;
            _loadBalancerOptionsCreator = loadBalancerOptionsCreator;
            _downstreamAddressesCreator = downstreamAddressesCreator;
            _headerFAndRCreator = headerFAndRCreator;
            _regionCreator = regionCreator;
            _rateLimitOptionsCreator = rateLimitOptionsCreator;
            _requestIdKeyCreator = requestIdKeyCreator;
            _upstreamTemplatePatternCreator = upstreamTemplatePatternCreator;
            _authOptionsCreator = authOptionsCreator;
            _claimsToThingCreator = claimsToThingCreator;
            _qosOptionsCreator = qosOptionsCreator;
            _fileRouteOptionsCreator = fileRouteOptionsCreator;
            _httpHandlerOptionsCreator = httpHandlerOptionsCreator;
            _loadBalancerOptionsCreator = loadBalancerOptionsCreator;
            _securityOptionsCreator = securityOptionsCreator;
            _versionCreator = versionCreator;
            _connectionCloseCreator = connectionCloseCreator;
        }

        public List<Route> Create(FileConfiguration fileConfiguration)
        {
            return fileConfiguration.Routes
                .Select(route =>
                {
                    var downstreamRoute = SetUpDownstreamRoute(route, fileConfiguration.GlobalConfiguration);
                    return SetUpRoute(route, downstreamRoute);
                })
                .ToList();
        }

        private DownstreamRoute SetUpDownstreamRoute(FileRoute fileRoute, FileGlobalConfiguration globalConfiguration)
        {
            var fileRouteOptions = _fileRouteOptionsCreator.Create(fileRoute);

            var requestIdKey = _requestIdKeyCreator.Create(fileRoute, globalConfiguration);

            var routeKey = _routeKeyCreator.Create(fileRoute);

            var upstreamTemplatePattern = _upstreamTemplatePatternCreator.Create(fileRoute);

            var authOptionsForRoute = _authOptionsCreator.Create(fileRoute);

            var claimsToHeaders = _claimsToThingCreator.Create(fileRoute.AddHeadersToRequest);

            var claimsToClaims = _claimsToThingCreator.Create(fileRoute.AddClaimsToRequest);

            var claimsToQueries = _claimsToThingCreator.Create(fileRoute.AddQueriesToRequest);

            var claimsToDownstreamPath = _claimsToThingCreator.Create(fileRoute.ChangeDownstreamPathTemplate);

            var qosOptions = _qosOptionsCreator.Create(fileRoute.QoSOptions, fileRoute.UpstreamPathTemplate, fileRoute.UpstreamHttpMethod);

            var rateLimitOption = _rateLimitOptionsCreator.Create(fileRoute.RateLimitOptions, globalConfiguration);

            var region = _regionCreator.Create(fileRoute);

            var httpHandlerOptions = _httpHandlerOptionsCreator.Create(fileRoute.HttpHandlerOptions);

            var hAndRs = _headerFAndRCreator.Create(fileRoute);

            var downstreamAddresses = _downstreamAddressesCreator.Create(fileRoute);

            var lbOptions = _loadBalancerOptionsCreator.Create(fileRoute.LoadBalancerOptions);

            var securityOptions = _securityOptionsCreator.Create(fileRoute.SecurityOptions);

            var downstreamHttpVersion = _versionCreator.Create(fileRoute.DownstreamHttpVersion);

            var connectionClose = _connectionCloseCreator.Create(fileRoute.ConnectionClose, globalConfiguration);

            var route = new DownstreamRouteBuilder()
                .WithKey(fileRoute.Key)
                .WithDownstreamPathTemplate(fileRoute.DownstreamPathTemplate)
                .WithUpstreamHttpMethod(fileRoute.UpstreamHttpMethod)
                .WithUpstreamPathTemplate(upstreamTemplatePattern)
                .WithIsAuthenticated(fileRouteOptions.IsAuthenticated)
                .WithAuthenticationOptions(authOptionsForRoute)
                .WithClaimsToHeaders(claimsToHeaders)
                .WithClaimsToClaims(claimsToClaims)
                .WithRouteClaimsRequirement(fileRoute.RouteClaimsRequirement)
                .WithIsAuthorized(fileRouteOptions.IsAuthorized)
                .WithClaimsToQueries(claimsToQueries)
                .WithClaimsToDownstreamPath(claimsToDownstreamPath)
                .WithRequestIdKey(requestIdKey)
                .WithIsCached(fileRouteOptions.IsCached)
                .WithCacheOptions(new CacheOptions(fileRoute.FileCacheOptions.TtlSeconds, region))
                .WithDownstreamScheme(fileRoute.DownstreamScheme)
                .WithLoadBalancerOptions(lbOptions)
                .WithDownstreamAddresses(downstreamAddresses)
                .WithLoadBalancerKey(routeKey)
                .WithQosOptions(qosOptions)
                .WithEnableRateLimiting(fileRouteOptions.EnableRateLimiting)
                .WithRateLimitOptions(rateLimitOption)
                .WithHttpHandlerOptions(httpHandlerOptions)
                .WithServiceName(fileRoute.ServiceName)
                .WithServiceNamespace(fileRoute.ServiceNamespace)
                .WithUseServiceDiscovery(fileRouteOptions.UseServiceDiscovery)
                .WithUpstreamHeaderFindAndReplace(hAndRs.Upstream)
                .WithDownstreamHeaderFindAndReplace(hAndRs.Downstream)
                .WithDelegatingHandlers(fileRoute.DelegatingHandlers)
                .WithAddHeadersToDownstream(hAndRs.AddHeadersToDownstream)
                .WithAddHeadersToUpstream(hAndRs.AddHeadersToUpstream)
                .WithDangerousAcceptAnyServerCertificateValidator(fileRoute.DangerousAcceptAnyServerCertificateValidator)
                .WithSecurityOptions(securityOptions)
                .WithDownstreamHttpVersion(downstreamHttpVersion)
                .WithDownStreamHttpMethod(fileRoute.DownstreamHttpMethod)
                .WithConnectionClose(connectionClose)
                .Build();

            return route;
        }

        private Route SetUpRoute(FileRoute fileRoute, DownstreamRoute downstreamRoutes)
        {
            var upstreamTemplatePattern = _upstreamTemplatePatternCreator.Create(fileRoute);

            var route = new RouteBuilder()
                .WithUpstreamHttpMethod(fileRoute.UpstreamHttpMethod)
                .WithUpstreamPathTemplate(upstreamTemplatePattern)
                .WithDownstreamRoute(downstreamRoutes)
                .WithUpstreamHost(fileRoute.UpstreamHost)
                .Build();

            return route;
        }
    }
}
