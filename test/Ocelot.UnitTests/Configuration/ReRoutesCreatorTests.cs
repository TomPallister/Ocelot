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

    public class ReRoutesCreatorTests
    {
        private ReRoutesCreator _creator;
        private Mock<IClaimsToThingCreator> _cthCreator;
        private Mock<IAuthenticationOptionsCreator> _aoCreator;
        private Mock<IUpstreamTemplatePatternCreator> _utpCreator;
        private Mock<IRequestIdKeyCreator> _ridkCreator;
        private Mock<IQoSOptionsCreator> _qosoCreator;
        private Mock<IReRouteOptionsCreator> _rroCreator;
        private Mock<IRateLimitOptionsCreator> _rloCreator;
        private Mock<IRegionCreator> _rCreator;
        private Mock<IHttpHandlerOptionsCreator> _hhoCreator;
        private Mock<IHeaderFindAndReplaceCreator> _hfarCreator;
        private Mock<IDownstreamAddressesCreator> _daCreator;
        private Mock<ILoadBalancerOptionsCreator> _lboCreator;
        private Mock<IReRouteKeyCreator> _rrkCreator;
        private Mock<ISecurityOptionsCreator> _soCreator;
        private Mock<IVersionCreator> _versionCreator;
        private FileConfiguration _fileConfig;
        private ReRouteOptions _rro;
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
        private List<ReRoute> _result;
        private SecurityOptions _securityOptions;
        private Version _expectedVersion;

        public ReRoutesCreatorTests()
        {
            _cthCreator = new Mock<IClaimsToThingCreator>();
            _aoCreator = new Mock<IAuthenticationOptionsCreator>();
            _utpCreator = new Mock<IUpstreamTemplatePatternCreator>();
            _ridkCreator = new Mock<IRequestIdKeyCreator>();
            _qosoCreator = new Mock<IQoSOptionsCreator>();
            _rroCreator = new Mock<IReRouteOptionsCreator>();
            _rloCreator = new Mock<IRateLimitOptionsCreator>();
            _rCreator = new Mock<IRegionCreator>();
            _hhoCreator = new Mock<IHttpHandlerOptionsCreator>();
            _hfarCreator = new Mock<IHeaderFindAndReplaceCreator>();
            _daCreator = new Mock<IDownstreamAddressesCreator>();
            _lboCreator = new Mock<ILoadBalancerOptionsCreator>();
            _rrkCreator = new Mock<IReRouteKeyCreator>();
            _soCreator = new Mock<ISecurityOptionsCreator>();
            _versionCreator = new Mock<IVersionCreator>();

            _creator = new ReRoutesCreator(
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
                .Then(_ => ThenNoReRoutesAreReturned())
                .BDDfy();
        }

        [Fact]
        public void should_return_re_routes()
        {
            var fileConfig = new FileConfiguration
            {
                ReRoutes = new List<FileReRoute>
                {
                    new FileReRoute
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
                    new FileReRoute
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
              .And(_ => ThenTheReRoutesAreCreated())
              .BDDfy();
        }

        private void ThenTheDependenciesAreCalledCorrectly()
        {
            ThenTheDepsAreCalledFor(_fileConfig.ReRoutes[0], _fileConfig.GlobalConfiguration);
            ThenTheDepsAreCalledFor(_fileConfig.ReRoutes[1], _fileConfig.GlobalConfiguration);
        }

        private void GivenTheDependenciesAreSetUpCorrectly()
        {
            _expectedVersion = new Version("1.1");
            _rro = new ReRouteOptions(false, false, false, false, false);
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

            _rroCreator.Setup(x => x.Create(It.IsAny<FileReRoute>())).Returns(_rro);
            _ridkCreator.Setup(x => x.Create(It.IsAny<FileReRoute>(), It.IsAny<FileGlobalConfiguration>())).Returns(_requestId);
            _rrkCreator.Setup(x => x.Create(It.IsAny<FileReRoute>())).Returns(_rrk);
            _utpCreator.Setup(x => x.Create(It.IsAny<IReRoute>())).Returns(_upt);
            _aoCreator.Setup(x => x.Create(It.IsAny<FileAuthenticationOptions>(), It.IsAny<FileAuthenticationOptions>())).Returns(_ao);
            _cthCreator.Setup(x => x.Create(It.IsAny<Dictionary<string, string>>())).Returns(_ctt);
            _qosoCreator.Setup(x => x.Create(It.IsAny<FileQoSOptions>(), It.IsAny<string>(), It.IsAny<List<string>>())).Returns(_qoso);
            _rloCreator.Setup(x => x.Create(It.IsAny<FileRateLimitRule>(), It.IsAny<FileGlobalConfiguration>())).Returns(_rlo);
            _rCreator.Setup(x => x.Create(It.IsAny<FileReRoute>())).Returns(_region);
            _hhoCreator.Setup(x => x.Create(It.IsAny<FileHttpHandlerOptions>())).Returns(_hho);
            _hfarCreator.Setup(x => x.Create(It.IsAny<FileReRoute>())).Returns(_ht);
            _daCreator.Setup(x => x.Create(It.IsAny<FileReRoute>())).Returns(_dhp);
            _lboCreator.Setup(x => x.Create(It.IsAny<FileLoadBalancerOptions>())).Returns(_lbo);
            _versionCreator.Setup(x => x.Create(It.IsAny<string>())).Returns(_expectedVersion);
        }

        private void ThenTheReRoutesAreCreated()
        {
            _result.Count.ShouldBe(2);

            ThenTheReRouteIsSet(_fileConfig.ReRoutes[0], 0);
            ThenTheReRouteIsSet(_fileConfig.ReRoutes[1], 1);
        }

        private void ThenNoReRoutesAreReturned()
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

        private void ThenTheReRouteIsSet(FileReRoute expected, int reRouteIndex)
        {
            _result[reRouteIndex].DownstreamReRoute[0].DownstreamHttpVersion.ShouldBe(_expectedVersion);
            _result[reRouteIndex].DownstreamReRoute[0].IsAuthenticated.ShouldBe(_rro.IsAuthenticated);
            _result[reRouteIndex].DownstreamReRoute[0].IsAuthorised.ShouldBe(_rro.IsAuthorised);
            _result[reRouteIndex].DownstreamReRoute[0].IsCached.ShouldBe(_rro.IsCached);
            _result[reRouteIndex].DownstreamReRoute[0].EnableEndpointEndpointRateLimiting.ShouldBe(_rro.EnableRateLimiting);
            _result[reRouteIndex].DownstreamReRoute[0].RequestIdKey.ShouldBe(_requestId);
            _result[reRouteIndex].DownstreamReRoute[0].LoadBalancerKey.ShouldBe(_rrk);
            _result[reRouteIndex].DownstreamReRoute[0].UpstreamPathTemplate.ShouldBe(_upt);
            _result[reRouteIndex].DownstreamReRoute[0].AuthenticationOptions.ShouldBe(_ao);
            _result[reRouteIndex].DownstreamReRoute[0].ClaimsToHeaders.ShouldBe(_ctt);
            _result[reRouteIndex].DownstreamReRoute[0].ClaimsToQueries.ShouldBe(_ctt);
            _result[reRouteIndex].DownstreamReRoute[0].ClaimsToClaims.ShouldBe(_ctt);
            _result[reRouteIndex].DownstreamReRoute[0].QosOptions.ShouldBe(_qoso);
            _result[reRouteIndex].DownstreamReRoute[0].RateLimitOptions.ShouldBe(_rlo);
            _result[reRouteIndex].DownstreamReRoute[0].CacheOptions.Region.ShouldBe(_region);
            _result[reRouteIndex].DownstreamReRoute[0].CacheOptions.TtlSeconds.ShouldBe(expected.FileCacheOptions.TtlSeconds);
            _result[reRouteIndex].DownstreamReRoute[0].HttpHandlerOptions.ShouldBe(_hho);
            _result[reRouteIndex].DownstreamReRoute[0].UpstreamHeadersFindAndReplace.ShouldBe(_ht.Upstream);
            _result[reRouteIndex].DownstreamReRoute[0].DownstreamHeadersFindAndReplace.ShouldBe(_ht.Downstream);
            _result[reRouteIndex].DownstreamReRoute[0].AddHeadersToUpstream.ShouldBe(_ht.AddHeadersToUpstream);
            _result[reRouteIndex].DownstreamReRoute[0].AddHeadersToDownstream.ShouldBe(_ht.AddHeadersToDownstream);
            _result[reRouteIndex].DownstreamReRoute[0].DownstreamAddresses.ShouldBe(_dhp);
            _result[reRouteIndex].DownstreamReRoute[0].LoadBalancerOptions.ShouldBe(_lbo);
            _result[reRouteIndex].DownstreamReRoute[0].UseServiceDiscovery.ShouldBe(_rro.UseServiceDiscovery);
            _result[reRouteIndex].DownstreamReRoute[0].DangerousAcceptAnyServerCertificateValidator.ShouldBe(expected.DangerousAcceptAnyServerCertificateValidator);
            _result[reRouteIndex].DownstreamReRoute[0].DelegatingHandlers.ShouldBe(expected.DelegatingHandlers);
            _result[reRouteIndex].DownstreamReRoute[0].ServiceName.ShouldBe(expected.ServiceName);
            _result[reRouteIndex].DownstreamReRoute[0].DownstreamScheme.ShouldBe(expected.DownstreamScheme);
            _result[reRouteIndex].DownstreamReRoute[0].RouteClaimsRequirement.ShouldBe(expected.RouteClaimsRequirement);
            _result[reRouteIndex].DownstreamReRoute[0].DownstreamPathTemplate.Value.ShouldBe(expected.DownstreamPathTemplate);
            _result[reRouteIndex].DownstreamReRoute[0].Key.ShouldBe(expected.Key);
            _result[reRouteIndex].UpstreamHttpMethod
                .Select(x => x.Method)
                .ToList()
                .ShouldContain(x => x == expected.UpstreamHttpMethod[0]);
            _result[reRouteIndex].UpstreamHttpMethod
                .Select(x => x.Method)
                .ToList()
                .ShouldContain(x => x == expected.UpstreamHttpMethod[1]);
            _result[reRouteIndex].UpstreamHost.ShouldBe(expected.UpstreamHost);
            _result[reRouteIndex].DownstreamReRoute.Count.ShouldBe(1);
            _result[reRouteIndex].UpstreamTemplatePattern.ShouldBe(_upt);
        }

        private void ThenTheDepsAreCalledFor(FileReRoute fileReRoute, FileGlobalConfiguration globalConfig)
        {
            _rroCreator.Verify(x => x.Create(fileReRoute), Times.Once);
            _ridkCreator.Verify(x => x.Create(fileReRoute, globalConfig), Times.Once);
            _rrkCreator.Verify(x => x.Create(fileReRoute), Times.Once);
            _utpCreator.Verify(x => x.Create(fileReRoute), Times.Exactly(2));
            _aoCreator.Verify(x => x.Create(fileReRoute.AuthenticationOptions, globalConfig.AuthenticationOptions), Times.Once);
            _cthCreator.Verify(x => x.Create(fileReRoute.AddHeadersToRequest), Times.Once);
            _cthCreator.Verify(x => x.Create(fileReRoute.AddClaimsToRequest), Times.Once);
            _cthCreator.Verify(x => x.Create(fileReRoute.AddQueriesToRequest), Times.Once);
            _qosoCreator.Verify(x => x.Create(fileReRoute.QoSOptions, fileReRoute.UpstreamPathTemplate, fileReRoute.UpstreamHttpMethod));
            _rloCreator.Verify(x => x.Create(fileReRoute.RateLimitOptions, globalConfig), Times.Once);
            _rCreator.Verify(x => x.Create(fileReRoute), Times.Once);
            _hhoCreator.Verify(x => x.Create(fileReRoute.HttpHandlerOptions), Times.Once);
            _hfarCreator.Verify(x => x.Create(fileReRoute), Times.Once);
            _daCreator.Verify(x => x.Create(fileReRoute), Times.Once);
            _lboCreator.Verify(x => x.Create(fileReRoute.LoadBalancerOptions), Times.Once);
            _soCreator.Verify(x => x.Create(fileReRoute.SecurityOptions), Times.Once);
        }
    }
}
