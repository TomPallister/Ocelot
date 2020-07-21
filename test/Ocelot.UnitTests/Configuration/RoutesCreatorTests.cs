namespace Ocelot.UnitTests.Configuration
{
    using System;
    using Moq;
    using Ocelot.Cache;
    using Ocelot.Configuration;
    using Ocelot.Configuration.Builder;
    using Ocelot.Configuration.Creator;
    using Ocelot.Configuration.File;
    using Ocelot.Values;
    using Shouldly;
    using System.Collections.Generic;
    using System.Linq;
    using TestStack.BDDfy;
    using Xunit;

    public class RoutesCreatorTests
    {
        private RoutesCreator _creator;
        private Mock<IClaimsToThingCreator> _cthCreator;
        private Mock<IAuthenticationOptionsCreator> _aoCreator;
        private Mock<IUpstreamTemplatePatternCreator> _utpCreator;
        private Mock<IRequestIdKeyCreator> _ridkCreator;
        private Mock<IQoSOptionsCreator> _qosoCreator;
        private Mock<IRouteOptionsCreator> _rroCreator;
        private Mock<IRateLimitOptionsCreator> _rloCreator;
        private Mock<IRegionCreator> _rCreator;
        private Mock<IHttpHandlerOptionsCreator> _hhoCreator;
        private Mock<IHeaderFindAndReplaceCreator> _hfarCreator;
        private Mock<IDownstreamAddressesCreator> _daCreator;
        private Mock<ILoadBalancerOptionsCreator> _lboCreator;
        private Mock<IRouteKeyCreator> _rrkCreator;
        private Mock<ISecurityOptionsCreator> _soCreator;
        private Mock<IVersionCreator> _versionCreator;
        private FileConfiguration _fileConfig;
        private RouteOptions _rro;
        private string _requestId;
        private string _rrk;
        private UpstreamPathTemplate _upt;
        private AuthenticationOptions _ao;
        private List<ClaimToThing> _ctt;
        private QoSOptions _qoso;
        private RateLimitOptions _rlo;
        private string _region;
        private HttpHandlerOptions _hho;
        private HeaderTransformations _ht;
        private List<DownstreamHostAndPort> _dhp;
        private LoadBalancerOptions _lbo;
        private List<Route> _result;
        private SecurityOptions _securityOptions;
        private Version _expectedVersion;

        public RoutesCreatorTests()
        {
            _cthCreator = new Mock<IClaimsToThingCreator>();
            _aoCreator = new Mock<IAuthenticationOptionsCreator>();
            _utpCreator = new Mock<IUpstreamTemplatePatternCreator>();
            _ridkCreator = new Mock<IRequestIdKeyCreator>();
            _qosoCreator = new Mock<IQoSOptionsCreator>();
            _rroCreator = new Mock<IRouteOptionsCreator>();
            _rloCreator = new Mock<IRateLimitOptionsCreator>();
            _rCreator = new Mock<IRegionCreator>();
            _hhoCreator = new Mock<IHttpHandlerOptionsCreator>();
            _hfarCreator = new Mock<IHeaderFindAndReplaceCreator>();
            _daCreator = new Mock<IDownstreamAddressesCreator>();
            _lboCreator = new Mock<ILoadBalancerOptionsCreator>();
            _rrkCreator = new Mock<IRouteKeyCreator>();
            _soCreator = new Mock<ISecurityOptionsCreator>();
            _versionCreator = new Mock<IVersionCreator>();

            _creator = new RoutesCreator(
                _cthCreator.Object,
                _aoCreator.Object,
                _utpCreator.Object,
                _ridkCreator.Object,
                _qosoCreator.Object,
                _rroCreator.Object,
                _rloCreator.Object,
                _rCreator.Object,
                _hhoCreator.Object,
                _hfarCreator.Object,
                _daCreator.Object,
                _lboCreator.Object,
                _rrkCreator.Object,
                _soCreator.Object,
                _versionCreator.Object
                );
        }

        [Fact]
        public void should_return_nothing()
        {
            var fileConfig = new FileConfiguration();

            this.Given(_ => GivenThe(fileConfig))
                .When(_ => WhenICreate())
                .Then(_ => ThenNoRoutesAreReturned())
                .BDDfy();
        }

        [Fact]
        public void should_return_re_routes()
        {
            var fileConfig = new FileConfiguration
            {
                Routes = new List<FileRoute>
                {
                    new FileRoute
                    {
                        ServiceName = "dave",
                        DangerousAcceptAnyServerCertificateValidator = true,
                        AddClaimsToRequest = new Dictionary<string, string>
                        {
                            { "a","b" }
                        },
                        AddHeadersToRequest = new Dictionary<string, string>
                        {
                            { "c","d" }
                        },
                        AddQueriesToRequest = new Dictionary<string, string>
                        {
                            { "e","f" }
                        },
                        UpstreamHttpMethod = new List<string> { "GET", "POST" }
                    },
                    new FileRoute
                    {
                        ServiceName = "wave",
                        DangerousAcceptAnyServerCertificateValidator = false,
                        AddClaimsToRequest = new Dictionary<string, string>
                        {
                            { "g","h" }
                        },
                        AddHeadersToRequest = new Dictionary<string, string>
                        {
                            { "i","j" }
                        },
                        AddQueriesToRequest = new Dictionary<string, string>
                        {
                            { "k","l" }
                        },
                        UpstreamHttpMethod = new List<string> { "PUT", "DELETE" }
                    }
                }
            };

            this.Given(_ => GivenThe(fileConfig))
              .And(_ => GivenTheDependenciesAreSetUpCorrectly())
              .When(_ => WhenICreate())
              .Then(_ => ThenTheDependenciesAreCalledCorrectly())
              .And(_ => ThenTheRoutesAreCreated())
              .BDDfy();
        }

        private void ThenTheDependenciesAreCalledCorrectly()
        {
            ThenTheDepsAreCalledFor(_fileConfig.Routes[0], _fileConfig.GlobalConfiguration);
            ThenTheDepsAreCalledFor(_fileConfig.Routes[1], _fileConfig.GlobalConfiguration);
        }

        private void GivenTheDependenciesAreSetUpCorrectly()
        {
            _expectedVersion = new Version("1.1");
            _rro = new RouteOptions(false, false, false, false, false);
            _requestId = "testy";
            _rrk = "besty";
            _upt = new UpstreamPathTemplateBuilder().Build();
            _ao = new AuthenticationOptionsBuilder().Build();
            _ctt = new List<ClaimToThing>();
            _qoso = new QoSOptionsBuilder().Build();
            _rlo = new RateLimitOptionsBuilder().Build();
            _region = "vesty";
            _hho = new HttpHandlerOptionsBuilder().Build();
            _ht = new HeaderTransformations(new List<HeaderFindAndReplace>(), new List<HeaderFindAndReplace>(), new List<AddHeader>(), new List<AddHeader>());
            _dhp = new List<DownstreamHostAndPort>();
            _lbo = new LoadBalancerOptionsBuilder().Build();

            _rroCreator.Setup(x => x.Create(It.IsAny<FileRoute>())).Returns(_rro);
            _ridkCreator.Setup(x => x.Create(It.IsAny<FileRoute>(), It.IsAny<FileGlobalConfiguration>())).Returns(_requestId);
            _rrkCreator.Setup(x => x.Create(It.IsAny<FileRoute>())).Returns(_rrk);
            _utpCreator.Setup(x => x.Create(It.IsAny<IRoute>())).Returns(_upt);
            _aoCreator.Setup(x => x.Create(It.IsAny<FileRoute>())).Returns(_ao);
            _cthCreator.Setup(x => x.Create(It.IsAny<Dictionary<string, string>>())).Returns(_ctt);
            _qosoCreator.Setup(x => x.Create(It.IsAny<FileQoSOptions>(), It.IsAny<string>(), It.IsAny<List<string>>())).Returns(_qoso);
            _rloCreator.Setup(x => x.Create(It.IsAny<FileRateLimitRule>(), It.IsAny<FileGlobalConfiguration>())).Returns(_rlo);
            _rCreator.Setup(x => x.Create(It.IsAny<FileRoute>())).Returns(_region);
            _hhoCreator.Setup(x => x.Create(It.IsAny<FileHttpHandlerOptions>())).Returns(_hho);
            _hfarCreator.Setup(x => x.Create(It.IsAny<FileRoute>())).Returns(_ht);
            _daCreator.Setup(x => x.Create(It.IsAny<FileRoute>())).Returns(_dhp);
            _lboCreator.Setup(x => x.Create(It.IsAny<FileLoadBalancerOptions>())).Returns(_lbo);
            _versionCreator.Setup(x => x.Create(It.IsAny<string>())).Returns(_expectedVersion);
        }

        private void ThenTheRoutesAreCreated()
        {
            _result.Count.ShouldBe(2);

            ThenTheRouteIsSet(_fileConfig.Routes[0], 0);
            ThenTheRouteIsSet(_fileConfig.Routes[1], 1);
        }

        private void ThenNoRoutesAreReturned()
        {
            _result.ShouldBeEmpty();
        }

        private void GivenThe(FileConfiguration fileConfig)
        {
            _fileConfig = fileConfig;
        }

        private void WhenICreate()
        {
            _result = _creator.Create(_fileConfig);
        }

        private void ThenTheRouteIsSet(FileRoute expected, int routeIndex)
        {
            _result[routeIndex].DownstreamRoute[0].DownstreamHttpVersion.ShouldBe(_expectedVersion);
            _result[routeIndex].DownstreamRoute[0].IsAuthenticated.ShouldBe(_rro.IsAuthenticated);
            _result[routeIndex].DownstreamRoute[0].IsAuthorised.ShouldBe(_rro.IsAuthorised);
            _result[routeIndex].DownstreamRoute[0].IsCached.ShouldBe(_rro.IsCached);
            _result[routeIndex].DownstreamRoute[0].EnableEndpointEndpointRateLimiting.ShouldBe(_rro.EnableRateLimiting);
            _result[routeIndex].DownstreamRoute[0].RequestIdKey.ShouldBe(_requestId);
            _result[routeIndex].DownstreamRoute[0].LoadBalancerKey.ShouldBe(_rrk);
            _result[routeIndex].DownstreamRoute[0].UpstreamPathTemplate.ShouldBe(_upt);
            _result[routeIndex].DownstreamRoute[0].AuthenticationOptions.ShouldBe(_ao);
            _result[routeIndex].DownstreamRoute[0].ClaimsToHeaders.ShouldBe(_ctt);
            _result[routeIndex].DownstreamRoute[0].ClaimsToQueries.ShouldBe(_ctt);
            _result[routeIndex].DownstreamRoute[0].ClaimsToClaims.ShouldBe(_ctt);
            _result[routeIndex].DownstreamRoute[0].QosOptions.ShouldBe(_qoso);
            _result[routeIndex].DownstreamRoute[0].RateLimitOptions.ShouldBe(_rlo);
            _result[routeIndex].DownstreamRoute[0].CacheOptions.Region.ShouldBe(_region);
            _result[routeIndex].DownstreamRoute[0].CacheOptions.TtlSeconds.ShouldBe(expected.FileCacheOptions.TtlSeconds);
            _result[routeIndex].DownstreamRoute[0].HttpHandlerOptions.ShouldBe(_hho);
            _result[routeIndex].DownstreamRoute[0].UpstreamHeadersFindAndReplace.ShouldBe(_ht.Upstream);
            _result[routeIndex].DownstreamRoute[0].DownstreamHeadersFindAndReplace.ShouldBe(_ht.Downstream);
            _result[routeIndex].DownstreamRoute[0].AddHeadersToUpstream.ShouldBe(_ht.AddHeadersToUpstream);
            _result[routeIndex].DownstreamRoute[0].AddHeadersToDownstream.ShouldBe(_ht.AddHeadersToDownstream);
            _result[routeIndex].DownstreamRoute[0].DownstreamAddresses.ShouldBe(_dhp);
            _result[routeIndex].DownstreamRoute[0].LoadBalancerOptions.ShouldBe(_lbo);
            _result[routeIndex].DownstreamRoute[0].UseServiceDiscovery.ShouldBe(_rro.UseServiceDiscovery);
            _result[routeIndex].DownstreamRoute[0].DangerousAcceptAnyServerCertificateValidator.ShouldBe(expected.DangerousAcceptAnyServerCertificateValidator);
            _result[routeIndex].DownstreamRoute[0].DelegatingHandlers.ShouldBe(expected.DelegatingHandlers);
            _result[routeIndex].DownstreamRoute[0].ServiceName.ShouldBe(expected.ServiceName);
            _result[routeIndex].DownstreamRoute[0].DownstreamScheme.ShouldBe(expected.DownstreamScheme);
            _result[routeIndex].DownstreamRoute[0].RouteClaimsRequirement.ShouldBe(expected.RouteClaimsRequirement);
            _result[routeIndex].DownstreamRoute[0].DownstreamPathTemplate.Value.ShouldBe(expected.DownstreamPathTemplate);
            _result[routeIndex].DownstreamRoute[0].Key.ShouldBe(expected.Key);
            _result[routeIndex].UpstreamHttpMethod
                .Select(x => x.Method)
                .ToList()
                .ShouldContain(x => x == expected.UpstreamHttpMethod[0]);
            _result[routeIndex].UpstreamHttpMethod
                .Select(x => x.Method)
                .ToList()
                .ShouldContain(x => x == expected.UpstreamHttpMethod[1]);
            _result[routeIndex].UpstreamHost.ShouldBe(expected.UpstreamHost);
            _result[routeIndex].DownstreamRoute.Count.ShouldBe(1);
            _result[routeIndex].UpstreamTemplatePattern.ShouldBe(_upt);
        }

        private void ThenTheDepsAreCalledFor(FileRoute fileRoute, FileGlobalConfiguration globalConfig)
        {
            _rroCreator.Verify(x => x.Create(fileRoute), Times.Once);
            _ridkCreator.Verify(x => x.Create(fileRoute, globalConfig), Times.Once);
            _rrkCreator.Verify(x => x.Create(fileRoute), Times.Once);
            _utpCreator.Verify(x => x.Create(fileRoute), Times.Exactly(2));
            _aoCreator.Verify(x => x.Create(fileRoute), Times.Once);
            _cthCreator.Verify(x => x.Create(fileRoute.AddHeadersToRequest), Times.Once);
            _cthCreator.Verify(x => x.Create(fileRoute.AddClaimsToRequest), Times.Once);
            _cthCreator.Verify(x => x.Create(fileRoute.AddQueriesToRequest), Times.Once);
            _qosoCreator.Verify(x => x.Create(fileRoute.QoSOptions, fileRoute.UpstreamPathTemplate, fileRoute.UpstreamHttpMethod));
            _rloCreator.Verify(x => x.Create(fileRoute.RateLimitOptions, globalConfig), Times.Once);
            _rCreator.Verify(x => x.Create(fileRoute), Times.Once);
            _hhoCreator.Verify(x => x.Create(fileRoute.HttpHandlerOptions), Times.Once);
            _hfarCreator.Verify(x => x.Create(fileRoute), Times.Once);
            _daCreator.Verify(x => x.Create(fileRoute), Times.Once);
            _lboCreator.Verify(x => x.Create(fileRoute.LoadBalancerOptions), Times.Once);
            _soCreator.Verify(x => x.Create(fileRoute.SecurityOptions), Times.Once);
        }
    }
}
