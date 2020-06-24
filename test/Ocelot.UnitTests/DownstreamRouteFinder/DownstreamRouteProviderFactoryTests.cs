namespace Ocelot.UnitTests.DownstreamRouteFinder
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using Ocelot.Configuration;
    using Ocelot.Configuration.Builder;
    using Ocelot.Configuration.Creator;
    using Ocelot.DownstreamRouteFinder.Finder;
    using Ocelot.DownstreamRouteFinder.UrlMatcher;
    using Ocelot.Logging;
    using Shouldly;
    using System.Collections.Generic;
    using TestStack.BDDfy;
    using Xunit;

    public class DownstreamRouteProviderFactoryTests
    {
        private readonly DownstreamRouteProviderFactory _factory;
        private IInternalConfiguration _config;
        private IDownstreamRouteProvider _result;
        private Mock<IOcelotLogger> _logger;
        private Mock<IOcelotLoggerFactory> _loggerFactory;

        public DownstreamRouteProviderFactoryTests()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IPlaceholderNameAndValueFinder, UrlPathPlaceholderNameAndValueFinder>();
            services.AddSingleton<IUrlPathToUrlTemplateMatcher, RegExUrlMatcher>();
            services.AddSingleton<IQoSOptionsCreator, QoSOptionsCreator>();
            services.AddSingleton<IDownstreamRouteProvider, DownstreamRouteFinder>();
            services.AddSingleton<IDownstreamRouteProvider, DownstreamRouteCreator>();
            var provider = services.BuildServiceProvider();
            _logger = new Mock<IOcelotLogger>();
            _loggerFactory = new Mock<IOcelotLoggerFactory>();
            _loggerFactory.Setup(x => x.CreateLogger<DownstreamRouteProviderFactory>()).Returns(_logger.Object);
            _factory = new DownstreamRouteProviderFactory(provider, _loggerFactory.Object);
        }

        [Fact]
        public void should_return_downstream_route_finder()
        {
            var routes = new List<Route>
            {
                new RouteBuilder().Build()
            };

            this.Given(_ => GivenTheRoutes(routes))
                .When(_ => WhenIGet())
                .Then(_ => ThenTheResultShouldBe<DownstreamRouteFinder>())
                .BDDfy();
        }

        [Fact]
        public void should_return_downstream_route_finder_when_not_dynamic_re_route_and_service_discovery_on()
        {
            var spConfig = new ServiceProviderConfigurationBuilder().WithScheme("http").WithHost("test").WithPort(50).WithType("test").Build();
            var routes = new List<Route>
            {
                new RouteBuilder().WithUpstreamPathTemplate(new UpstreamPathTemplateBuilder().WithOriginalValue("woot").Build()).Build()
            };

            this.Given(_ => GivenTheRoutes(routes, spConfig))
                .When(_ => WhenIGet())
                .Then(_ => ThenTheResultShouldBe<DownstreamRouteFinder>())
                .BDDfy();
        }

        [Fact]
        public void should_return_downstream_route_finder_as_no_service_discovery_given_no_scheme()
        {
            var spConfig = new ServiceProviderConfigurationBuilder().WithScheme("").WithHost("test").WithPort(50).Build();
            var routes = new List<Route>();

            this.Given(_ => GivenTheRoutes(routes, spConfig))
                .When(_ => WhenIGet())
                .Then(_ => ThenTheResultShouldBe<DownstreamRouteFinder>())
                .BDDfy();
        }

        [Fact]
        public void should_return_downstream_route_finder_as_no_service_discovery_given_no_host()
        {
            var spConfig = new ServiceProviderConfigurationBuilder().WithScheme("http").WithHost("").WithPort(50).Build();
            var routes = new List<Route>();

            this.Given(_ => GivenTheRoutes(routes, spConfig))
                .When(_ => WhenIGet())
                .Then(_ => ThenTheResultShouldBe<DownstreamRouteFinder>())
                .BDDfy();
        }

        [Fact]
        public void should_return_downstream_route_finder_given_no_service_discovery_port()
        {
            var spConfig = new ServiceProviderConfigurationBuilder().WithScheme("http").WithHost("localhost").WithPort(0).Build();
            var routes = new List<Route>();

            this.Given(_ => GivenTheRoutes(routes, spConfig))
                .When(_ => WhenIGet())
                .Then(_ => ThenTheResultShouldBe<DownstreamRouteFinder>())
                .BDDfy();
        }

        [Fact]
        public void should_return_downstream_route_finder_given_no_service_discovery_type()
        {
            var spConfig = new ServiceProviderConfigurationBuilder().WithScheme("http").WithHost("localhost").WithPort(50).WithType("").Build();
            var routes = new List<Route>();

            this.Given(_ => GivenTheRoutes(routes, spConfig))
                .When(_ => WhenIGet())
                .Then(_ => ThenTheResultShouldBe<DownstreamRouteFinder>())
                .BDDfy();
        }

        [Fact]
        public void should_return_downstream_route_creator()
        {
            var spConfig = new ServiceProviderConfigurationBuilder().WithScheme("http").WithHost("test").WithPort(50).WithType("test").Build();
            var routes = new List<Route>();

            this.Given(_ => GivenTheRoutes(routes, spConfig))
                .When(_ => WhenIGet())
                .Then(_ => ThenTheResultShouldBe<DownstreamRouteCreator>())
                .BDDfy();
        }

        [Fact]
        public void should_return_downstream_route_creator_with_dynamic_re_route()
        {
            var spConfig = new ServiceProviderConfigurationBuilder().WithScheme("http").WithHost("test").WithPort(50).WithType("test").Build();
            var routes = new List<Route>
            {
                new RouteBuilder().Build()
            };

            this.Given(_ => GivenTheRoutes(routes, spConfig))
                .When(_ => WhenIGet())
                .Then(_ => ThenTheResultShouldBe<DownstreamRouteCreator>())
                .BDDfy();
        }

        private void ThenTheResultShouldBe<T>()
        {
            _result.ShouldBeOfType<T>();
        }

        private void WhenIGet()
        {
            _result = _factory.Get(_config);
        }

        private void GivenTheRoutes(List<Route> routes)
        {
            _config = new InternalConfiguration(routes, "", null, "", new LoadBalancerOptionsBuilder().Build(), "", new QoSOptionsBuilder().Build(), new HttpHandlerOptionsBuilder().Build(), new Version("1.1"));
        }

        private void GivenTheRoutes(List<Route> routes, ServiceProviderConfiguration config)
        {
            _config = new InternalConfiguration(routes, "", config, "", new LoadBalancerOptionsBuilder().Build(), "", new QoSOptionsBuilder().Build(), new HttpHandlerOptionsBuilder().Build(), new Version("1.1"));
        }
    }
}
