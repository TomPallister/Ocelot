using Ocelot.Configuration.Creator;
using Ocelot.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Ocelot.Configuration.Builder
{
    public class DownstreamRouteBuilder
    {
        private AuthenticationOptions _authenticationOptions;
        private string _loadBalancerKey;
        private string _downstreamPathTemplate;
        private UpstreamPathTemplate _upstreamTemplatePattern;
        private List<HttpMethod> _upstreamHttpMethod;
        private bool _isAuthenticated;
        private List<ClaimToThing> _claimsToHeaders;
        private List<ClaimToThing> _claimToClaims;
        private Dictionary<string, string> _routeClaimRequirement;
        private bool _isAuthorised;
        private List<ClaimToThing> _claimToQueries;
        private List<ClaimToThing> _claimToDownstreamPath;
        private string _requestIdHeaderKey;
        private bool _isCached;
        private CacheOptions _fileCacheOptions;
        private string _downstreamScheme;
        private LoadBalancerOptions _loadBalancerOptions;
        private QoSOptions _qosOptions;
        private HttpHandlerOptions _httpHandlerOptions;
        private bool _enableRateLimiting;
        private RateLimitOptions _rateLimitOptions;
        private bool _useServiceDiscovery;
        private string _serviceName;
        private string _serviceNamespace;
        private List<HeaderFindAndReplace> _upstreamHeaderFindAndReplace;
        private List<HeaderFindAndReplace> _downstreamHeaderFindAndReplace;
        private readonly List<DownstreamHostAndPort> _downstreamAddresses;
        private string _key;
        private List<string> _delegatingHandlers;
        private List<AddHeader> _addHeadersToDownstream;
        private List<AddHeader> _addHeadersToUpstream;
        private bool _dangerousAcceptAnyServerCertificateValidator;
        private SecurityOptions _securityOptions;
        private string _downstreamHttpMethod;
        private Version _downstreamHttpVersion;

        public DownstreamRouteBuilder()
        {
            _downstreamAddresses = new List<DownstreamHostAndPort>();
            _delegatingHandlers = new List<string>();
            _addHeadersToDownstream = new List<AddHeader>();
            _addHeadersToUpstream = new List<AddHeader>();
        }

        public DownstreamRouteBuilder WithDownstreamAddresses(List<DownstreamHostAndPort> downstreamAddresses)
        {
            _downstreamAddresses.AddRange(downstreamAddresses);
            return this;
        }

        public DownstreamRouteBuilder WithDownStreamHttpMethod(string method)
        {
            _downstreamHttpMethod = method;
            return this;
        }

        public DownstreamRouteBuilder WithLoadBalancerOptions(LoadBalancerOptions loadBalancerOptions)
        {
            _loadBalancerOptions = loadBalancerOptions;
            return this;
        }

        public DownstreamRouteBuilder WithDownstreamScheme(string downstreamScheme)
        {
            _downstreamScheme = downstreamScheme;
            return this;
        }

        public DownstreamRouteBuilder WithDownstreamPathTemplate(string input)
        {
            _downstreamPathTemplate = input;
            return this;
        }

        public DownstreamRouteBuilder WithUpstreamPathTemplate(UpstreamPathTemplate input)
        {
            _upstreamTemplatePattern = input;
            return this;
        }

        public DownstreamRouteBuilder WithUpstreamHttpMethod(List<string> input)
        {
            _upstreamHttpMethod = (input.Count == 0) ? new List<HttpMethod>() : input.Select(x => new HttpMethod(x.Trim())).ToList();
            return this;
        }

        public DownstreamRouteBuilder WithIsAuthenticated(bool input)
        {
            _isAuthenticated = input;
            return this;
        }

        public DownstreamRouteBuilder WithIsAuthorised(bool input)
        {
            _isAuthorised = input;
            return this;
        }

        public DownstreamRouteBuilder WithRequestIdKey(string input)
        {
            _requestIdHeaderKey = input;
            return this;
        }

        public DownstreamRouteBuilder WithClaimsToHeaders(List<ClaimToThing> input)
        {
            _claimsToHeaders = input;
            return this;
        }

        public DownstreamRouteBuilder WithClaimsToClaims(List<ClaimToThing> input)
        {
            _claimToClaims = input;
            return this;
        }

        public DownstreamRouteBuilder WithRouteClaimsRequirement(Dictionary<string, string> input)
        {
            _routeClaimRequirement = input;
            return this;
        }

        public DownstreamRouteBuilder WithClaimsToQueries(List<ClaimToThing> input)
        {
            _claimToQueries = input;
            return this;
        }

        public DownstreamRouteBuilder WithClaimsToDownstreamPath(List<ClaimToThing> input)
        {
            _claimToDownstreamPath = input;
            return this;
        }

        public DownstreamRouteBuilder WithIsCached(bool input)
        {
            _isCached = input;
            return this;
        }

        public DownstreamRouteBuilder WithCacheOptions(CacheOptions input)
        {
            _fileCacheOptions = input;
            return this;
        }

        public DownstreamRouteBuilder WithQosOptions(QoSOptions input)
        {
            _qosOptions = input;
            return this;
        }

        public DownstreamRouteBuilder WithLoadBalancerKey(string loadBalancerKey)
        {
            _loadBalancerKey = loadBalancerKey;
            return this;
        }

        public DownstreamRouteBuilder WithAuthenticationOptions(AuthenticationOptions authenticationOptions)
        {
            _authenticationOptions = authenticationOptions;
            return this;
        }

        public DownstreamRouteBuilder WithEnableRateLimiting(bool input)
        {
            _enableRateLimiting = input;
            return this;
        }

        public DownstreamRouteBuilder WithRateLimitOptions(RateLimitOptions input)
        {
            _rateLimitOptions = input;
            return this;
        }

        public DownstreamRouteBuilder WithHttpHandlerOptions(HttpHandlerOptions input)
        {
            _httpHandlerOptions = input;
            return this;
        }

        public DownstreamRouteBuilder WithUseServiceDiscovery(bool useServiceDiscovery)
        {
            _useServiceDiscovery = useServiceDiscovery;
            return this;
        }

        public DownstreamRouteBuilder WithServiceName(string serviceName)
        {
            _serviceName = serviceName;
            return this;
        }

        public DownstreamRouteBuilder WithServiceNamespace(string serviceNamespace)
        {
            _serviceNamespace = serviceNamespace;
            return this;
        }

        public DownstreamRouteBuilder WithUpstreamHeaderFindAndReplace(List<HeaderFindAndReplace> upstreamHeaderFindAndReplace)
        {
            _upstreamHeaderFindAndReplace = upstreamHeaderFindAndReplace;
            return this;
        }

        public DownstreamRouteBuilder WithDownstreamHeaderFindAndReplace(List<HeaderFindAndReplace> downstreamHeaderFindAndReplace)
        {
            _downstreamHeaderFindAndReplace = downstreamHeaderFindAndReplace;
            return this;
        }

        public DownstreamRouteBuilder WithKey(string key)
        {
            _key = key;
            return this;
        }

        public DownstreamRouteBuilder WithDelegatingHandlers(List<string> delegatingHandlers)
        {
            _delegatingHandlers = delegatingHandlers;
            return this;
        }

        public DownstreamRouteBuilder WithAddHeadersToDownstream(List<AddHeader> addHeadersToDownstream)
        {
            _addHeadersToDownstream = addHeadersToDownstream;
            return this;
        }

        public DownstreamRouteBuilder WithAddHeadersToUpstream(List<AddHeader> addHeadersToUpstream)
        {
            _addHeadersToUpstream = addHeadersToUpstream;
            return this;
        }

        public DownstreamRouteBuilder WithDangerousAcceptAnyServerCertificateValidator(bool dangerousAcceptAnyServerCertificateValidator)
        {
            _dangerousAcceptAnyServerCertificateValidator = dangerousAcceptAnyServerCertificateValidator;
            return this;
        }

        public DownstreamRouteBuilder WithSecurityOptions(SecurityOptions securityOptions)
        {
            _securityOptions = securityOptions;
            return this;
        }

        public DownstreamRouteBuilder WithDownstreamHttpVersion(Version downstreamHttpVersion)
        {
            _downstreamHttpVersion = downstreamHttpVersion;
            return this;
        }

        public DownstreamRoute Build()
        {
            return new DownstreamRoute(
                _key,
                _upstreamTemplatePattern,
                _upstreamHeaderFindAndReplace,
                _downstreamHeaderFindAndReplace,
                _downstreamAddresses,
                _serviceName,
                _serviceNamespace,
                _httpHandlerOptions,
                _useServiceDiscovery,
                _enableRateLimiting,
                _qosOptions,
                _downstreamScheme,
                _requestIdHeaderKey,
                _isCached,
                _fileCacheOptions,
                _loadBalancerOptions,
                _rateLimitOptions,
                _routeClaimRequirement,
                _claimToQueries,
                _claimsToHeaders,
                _claimToClaims,
                _claimToDownstreamPath,
                _isAuthenticated,
                _isAuthorised,
                _authenticationOptions,
                new DownstreamPathTemplate(_downstreamPathTemplate),
                _loadBalancerKey,
                _delegatingHandlers,
                _addHeadersToDownstream,
                _addHeadersToUpstream,
                _dangerousAcceptAnyServerCertificateValidator,
                _securityOptions,
                _downstreamHttpMethod,
                _downstreamHttpVersion);
        }
    }
}
