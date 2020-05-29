namespace Ocelot.UnitTests.LoadBalancer
{
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Ocelot.Configuration;
    using Ocelot.Configuration.Builder;
    using Ocelot.Errors;
    using Ocelot.LoadBalancer.LoadBalancers;
    using Ocelot.LoadBalancer.Middleware;
    using Ocelot.Logging;
    using Ocelot.Request.Middleware;
    using Ocelot.Responses;
    using Ocelot.Values;
    using Shouldly;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Ocelot.Infrastructure.RequestData;
    using TestStack.BDDfy;
    using Xunit;
    using System;
    using System.Linq.Expressions;
    using Ocelot.Middleware;
    using Ocelot.DownstreamRouteFinder.Middleware;

    public class LoadBalancerMiddlewareTests
    {
        private readonly Mock<ILoadBalancerHouse> _loadBalancerHouse;
        private readonly Mock<ILoadBalancer> _loadBalancer;
        private ServiceHostAndPort _hostAndPort;
        private ErrorResponse<ILoadBalancer> _getLoadBalancerHouseError;
        private ErrorResponse<ServiceHostAndPort> _getHostAndPortError;
        private HttpRequestMessage _downstreamRequest;
        private ServiceProviderConfiguration _config;
        private Mock<IOcelotLoggerFactory> _loggerFactory;
        private Mock<IOcelotLogger> _logger;
        private LoadBalancingMiddleware _middleware;
        private RequestDelegate _next;
        private HttpContext _httpContext;
        private Mock<IRequestScopedDataRepository> _repo;

        public LoadBalancerMiddlewareTests()
        {
            _repo = new Mock<IRequestScopedDataRepository>();
            _httpContext = new DefaultHttpContext();
            _loadBalancerHouse = new Mock<ILoadBalancerHouse>();
            _loadBalancer = new Mock<ILoadBalancer>();
            _loadBalancerHouse = new Mock<ILoadBalancerHouse>();
            _downstreamRequest = new HttpRequestMessage(HttpMethod.Get, "http://test.com/");
            _loggerFactory = new Mock<IOcelotLoggerFactory>();
            _logger = new Mock<IOcelotLogger>();
            _loggerFactory.Setup(x => x.CreateLogger<LoadBalancingMiddleware>()).Returns(_logger.Object);
            _next = context => Task.CompletedTask;
        }

        [Fact]
        public void should_call_scoped_data_repository_correctly()
        {
            var downstreamRoute = new DownstreamRouteBuilder()
                    .WithUpstreamHttpMethod(new List<string> { "Get" })
                    .Build();

            var serviceProviderConfig = new ServiceProviderConfigurationBuilder()
                .Build();

            this.Given(x => x.GivenTheDownStreamUrlIs("http://my.url/abc?q=123"))
                .And(x => GivenTheConfigurationIs(serviceProviderConfig))
                .And(x => x.GivenTheDownStreamRouteIs(downstreamRoute, new List<Ocelot.DownstreamRouteFinder.UrlMatcher.PlaceholderNameAndValue>()))
                .And(x => x.GivenTheLoadBalancerHouseReturns())
                .And(x => x.GivenTheLoadBalancerReturns())
                .When(x => x.WhenICallTheMiddleware())
                .Then(x => x.ThenTheDownstreamUrlIsReplacedWith("http://127.0.0.1:80/abc?q=123"))
                .BDDfy();
        }

        [Fact]
        public void should_set_pipeline_error_if_cannot_get_load_balancer()
        {
            var downstreamRoute = new DownstreamRouteBuilder()
                    .WithUpstreamHttpMethod(new List<string> { "Get" })
                    .Build();

            var serviceProviderConfig = new ServiceProviderConfigurationBuilder()
                .Build();

            this.Given(x => x.GivenTheDownStreamUrlIs("http://my.url/abc?q=123"))
                .And(x => GivenTheConfigurationIs(serviceProviderConfig))
                .And(x => x.GivenTheDownStreamRouteIs(downstreamRoute, new List<Ocelot.DownstreamRouteFinder.UrlMatcher.PlaceholderNameAndValue>()))
                .And(x => x.GivenTheLoadBalancerHouseReturnsAnError())
                .When(x => x.WhenICallTheMiddleware())
                .Then(x => x.ThenAnErrorStatingLoadBalancerCouldNotBeFoundIsSetOnPipeline())
                .BDDfy();
        }

        [Fact]
        public void should_set_pipeline_error_if_cannot_get_least()
        {
            var downstreamRoute = new DownstreamRouteBuilder()
                    .WithUpstreamHttpMethod(new List<string> { "Get" })
                    .Build();

            var serviceProviderConfig = new ServiceProviderConfigurationBuilder()
               .Build();

            this.Given(x => x.GivenTheDownStreamUrlIs("http://my.url/abc?q=123"))
                .And(x => GivenTheConfigurationIs(serviceProviderConfig))
                .And(x => x.GivenTheDownStreamRouteIs(downstreamRoute, new List<Ocelot.DownstreamRouteFinder.UrlMatcher.PlaceholderNameAndValue>()))
                .And(x => x.GivenTheLoadBalancerHouseReturns())
                .And(x => x.GivenTheLoadBalancerReturnsAnError())
                .When(x => x.WhenICallTheMiddleware())
                .Then(x => x.ThenAnErrorStatingHostAndPortCouldNotBeFoundIsSetOnPipeline())
                .BDDfy();
        }

        [Fact]
        public void should_set_scheme()
        {
            var downstreamRoute = new DownstreamRouteBuilder()
                .WithUpstreamHttpMethod(new List<string> { "Get" })
                .Build();

            var serviceProviderConfig = new ServiceProviderConfigurationBuilder()
                .Build();

            this.Given(x => x.GivenTheDownStreamUrlIs("http://my.url/abc?q=123"))
                .And(x => GivenTheConfigurationIs(serviceProviderConfig))
                .And(x => x.GivenTheDownStreamRouteIs(downstreamRoute, new List<Ocelot.DownstreamRouteFinder.UrlMatcher.PlaceholderNameAndValue>()))
                .And(x => x.GivenTheLoadBalancerHouseReturns())
                .And(x => x.GivenTheLoadBalancerReturnsOk())
                .When(x => x.WhenICallTheMiddleware())
                .Then(x => x.ThenAnHostAndPortIsSetOnPipeline())
                .BDDfy();
        }

        private void WhenICallTheMiddleware()
        {
            _middleware = new LoadBalancingMiddleware(_next, _loggerFactory.Object, _loadBalancerHouse.Object);
            _middleware.Invoke(_httpContext).GetAwaiter().GetResult();
        }

        private void GivenTheConfigurationIs(ServiceProviderConfiguration config)
        {
            _config = config;
            var configuration = new InternalConfiguration(null, null, config, null, null, null, null, null, null);
            _httpContext.Items.SetIInternalConfiguration(configuration);
        }

        private void GivenTheDownStreamUrlIs(string downstreamUrl)
        {
            _downstreamRequest.RequestUri = new Uri(downstreamUrl);
            _httpContext.Items.UpsertDownstreamRequest(new DownstreamRequest(_downstreamRequest));
        }

        private void GivenTheLoadBalancerReturnsAnError()
        {
            _getHostAndPortError = new ErrorResponse<ServiceHostAndPort>(new List<Error>() { new ServicesAreNullError($"services were null for bah") });
            _loadBalancer
               .Setup(x => x.Lease(It.IsAny<HttpContext>()))
               .ReturnsAsync(_getHostAndPortError);
        }

        private void GivenTheLoadBalancerReturnsOk()
        {
            _loadBalancer
                .Setup(x => x.Lease(It.IsAny<HttpContext>()))
                .ReturnsAsync(new OkResponse<ServiceHostAndPort>(new ServiceHostAndPort("abc", 123, "https")));
        }

        private void GivenTheLoadBalancerReturns()
        {
            _hostAndPort = new ServiceHostAndPort("127.0.0.1", 80);
            _loadBalancer
                .Setup(x => x.Lease(It.IsAny<HttpContext>()))
                .ReturnsAsync(new OkResponse<ServiceHostAndPort>(_hostAndPort));
        }

        private void GivenTheDownStreamRouteIs(DownstreamRoute downstreamRoute, List<Ocelot.DownstreamRouteFinder.UrlMatcher.PlaceholderNameAndValue> placeholder)
        {
            _httpContext.Items.UpsertTemplatePlaceholderNameAndValues(placeholder);
            _httpContext.Items.UpsertDownstreamRoute(downstreamRoute);
        }

        private void GivenTheLoadBalancerHouseReturns()
        {
            _loadBalancerHouse
                .Setup(x => x.Get(It.IsAny<DownstreamRoute>(), It.IsAny<ServiceProviderConfiguration>()))
                .Returns(new OkResponse<ILoadBalancer>(_loadBalancer.Object));
        }

        private void GivenTheLoadBalancerHouseReturnsAnError()
        {
            _getLoadBalancerHouseError = new ErrorResponse<ILoadBalancer>(new List<Ocelot.Errors.Error>()
            {
                new UnableToFindLoadBalancerError($"unabe to find load balancer for bah")
            });

            _loadBalancerHouse
                .Setup(x => x.Get(It.IsAny<DownstreamRoute>(), It.IsAny<ServiceProviderConfiguration>()))
                .Returns(_getLoadBalancerHouseError);
        }

        private void ThenAnErrorStatingLoadBalancerCouldNotBeFoundIsSetOnPipeline()
        {
            _httpContext.Items.Errors().Count.ShouldBeGreaterThan(0);
            _httpContext.Items.Errors().ShouldBe(_getLoadBalancerHouseError.Errors);
        }

        private void ThenAnErrorSayingReleaseFailedIsSetOnThePipeline()
        {
            _httpContext.Items.Errors().Count.ShouldBeGreaterThan(0);
            _httpContext.Items.Errors().ShouldBe(It.IsAny<List<Error>>());
        }

        private void ThenAnErrorStatingHostAndPortCouldNotBeFoundIsSetOnPipeline()
        {
            _httpContext.Items.Errors().Count.ShouldBeGreaterThan(0);
            _httpContext.Items.Errors().ShouldBe(_getHostAndPortError.Errors);
        }

        private void ThenAnHostAndPortIsSetOnPipeline()
        {
            _httpContext.Items.DownstreamRequest().Host.ShouldBeEquivalentTo("abc");
            _httpContext.Items.DownstreamRequest().Port.ShouldBeEquivalentTo(123);
            _httpContext.Items.DownstreamRequest().Scheme.ShouldBeEquivalentTo("https");
        }

        private void ThenTheDownstreamUrlIsReplacedWith(string expectedUri)
        {
            _httpContext.Items.DownstreamRequest().ToHttpRequestMessage().RequestUri.OriginalString.ShouldBe(expectedUri);
        }
    }
}
