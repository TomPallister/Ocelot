namespace Ocelot.UnitTests.DownstreamRouteFinder
{
    using System;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Ocelot.Configuration;
    using Ocelot.Configuration.Builder;
    using Ocelot.DownstreamRouteFinder;
    using Ocelot.DownstreamRouteFinder.Finder;
    using Ocelot.DownstreamRouteFinder.Middleware;
    using Ocelot.DownstreamRouteFinder.UrlMatcher;
    using Ocelot.Logging;
    using Ocelot.Middleware;
    using Ocelot.Multiplexer;
    using Ocelot.Responses;
    using Shouldly;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Ocelot.Infrastructure.RequestData;
    using TestStack.BDDfy;
    using Xunit;

    public class DownstreamRouteFinderMiddlewareTests
    {
        private readonly Mock<IDownstreamRouteProvider> _finder;
        private readonly Mock<IDownstreamRouteProviderFactory> _factory;
        private Response<Ocelot.DownstreamRouteFinder.DownstreamRouteHolder> _downstreamRoute;
        private IInternalConfiguration _config;
        private Mock<IOcelotLoggerFactory> _loggerFactory;
        private Mock<IOcelotLogger> _logger;
        private readonly DownstreamRouteFinderMiddleware _middleware;
        private RequestDelegate _next;
        private HttpContext _httpContext;

        public DownstreamRouteFinderMiddlewareTests()
        {
            _httpContext = new DefaultHttpContext();
            _finder = new Mock<IDownstreamRouteProvider>();
            _factory = new Mock<IDownstreamRouteProviderFactory>();
            _factory.Setup(x => x.Get(It.IsAny<IInternalConfiguration>())).Returns(_finder.Object);
            _loggerFactory = new Mock<IOcelotLoggerFactory>();
            _logger = new Mock<IOcelotLogger>();
            _loggerFactory.Setup(x => x.CreateLogger<DownstreamRouteFinderMiddleware>()).Returns(_logger.Object);
            _next = context => Task.CompletedTask;
            _middleware = new DownstreamRouteFinderMiddleware(_next, _loggerFactory.Object, _factory.Object);
        }

        [Fact]
        public void should_call_scoped_data_repository_correctly()
        {
            var config = new InternalConfiguration(null, null, new ServiceProviderConfigurationBuilder().Build(), "", new LoadBalancerOptionsBuilder().Build(), "", new QoSOptionsBuilder().Build(), new HttpHandlerOptionsBuilder().Build(), new Version("1.1"));

            var downstreamRoute = new DownstreamRouteBuilder()
                .WithDownstreamPathTemplate("any old string")
                .WithUpstreamHttpMethod(new List<string> { "Get" })
                .Build();

            this.Given(x => x.GivenTheDownStreamRouteFinderReturns(
                new DownstreamRouteHolder(
                    new List<PlaceholderNameAndValue>(),
                    new RouteBuilder()
                        .WithDownstreamRoute(downstreamRoute)
                        .WithUpstreamHttpMethod(new List<string> { "Get" })
                        .Build())))
                .And(x => GivenTheFollowingConfig(config))
                .When(x => x.WhenICallTheMiddleware())
                .Then(x => x.ThenTheScopedDataRepositoryIsCalledCorrectly())
                .BDDfy();
        }

        private void WhenICallTheMiddleware()
        {
            _middleware.Invoke(_httpContext).GetAwaiter().GetType();
        }

        private void GivenTheFollowingConfig(IInternalConfiguration config)
        {
            _config = config;
            _httpContext.Items.SetIInternalConfiguration(config);
        }

        private void GivenTheDownStreamRouteFinderReturns(Ocelot.DownstreamRouteFinder.DownstreamRouteHolder downstreamRoute)
        {
            _downstreamRoute = new OkResponse<Ocelot.DownstreamRouteFinder.DownstreamRouteHolder>(downstreamRoute);
            _finder
                .Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IInternalConfiguration>(), It.IsAny<string>()))
                .Returns(_downstreamRoute);
        }

        private void ThenTheScopedDataRepositoryIsCalledCorrectly()
        {
            _httpContext.Items.TemplatePlaceholderNameAndValues().ShouldBe(_downstreamRoute.Data.TemplatePlaceholderNameAndValues);
            _httpContext.Items.IInternalConfiguration().ServiceProviderConfiguration.ShouldBe(_config.ServiceProviderConfiguration);
        }
    }
}
