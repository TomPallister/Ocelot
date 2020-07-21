namespace Ocelot.UnitTests.DownstreamUrlCreator
{
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Ocelot.Configuration;
    using Ocelot.Configuration.Builder;
    using Ocelot.DownstreamRouteFinder;
    using Ocelot.DownstreamRouteFinder.UrlMatcher;
    using Ocelot.DownstreamUrlCreator.Middleware;
    using Ocelot.DownstreamUrlCreator.UrlTemplateReplacer;
    using Ocelot.Logging;
    using Ocelot.Middleware;
    using Ocelot.Request.Middleware;
    using Ocelot.Responses;
    using Ocelot.Values;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Ocelot.Infrastructure.RequestData;
    using TestStack.BDDfy;
    using Xunit;
    using Ocelot.DownstreamRouteFinder.Middleware;

    public class DownstreamUrlCreatorMiddlewareTests
    {
        private readonly Mock<IDownstreamPathPlaceholderReplacer> _downstreamUrlTemplateVariableReplacer;
        private OkResponse<DownstreamPath> _downstreamPath;
        private readonly Mock<IOcelotLoggerFactory> _loggerFactory;
        private Mock<IOcelotLogger> _logger;
        private DownstreamUrlCreatorMiddleware _middleware;
        private readonly RequestDelegate _next;
        private readonly HttpRequestMessage _request;
        private HttpContext _httpContext;
        private Mock<IRequestScopedDataRepository> _repo;

        public DownstreamUrlCreatorMiddlewareTests()
        {
            _repo = new Mock<IRequestScopedDataRepository>();
            _httpContext = new DefaultHttpContext();
            _loggerFactory = new Mock<IOcelotLoggerFactory>();
            _logger = new Mock<IOcelotLogger>();
            _loggerFactory.Setup(x => x.CreateLogger<DownstreamUrlCreatorMiddleware>()).Returns(_logger.Object);
            _downstreamUrlTemplateVariableReplacer = new Mock<IDownstreamPathPlaceholderReplacer>();
            _request = new HttpRequestMessage(HttpMethod.Get, "https://my.url/abc/?q=123");
            _next = context => Task.CompletedTask;
        }

        [Fact]
        public void should_replace_scheme_and_path()
        {
            var downstreamRoute = new DownstreamRouteBuilder()
                .WithDownstreamPathTemplate("any old string")
                .WithUpstreamHttpMethod(new List<string> { "Get" })
                .WithDownstreamScheme("https")
                .Build();

            var config = new ServiceProviderConfigurationBuilder()
                .Build();

            this.Given(x => x.GivenTheDownStreamRouteIs(
                    new DownstreamRouteHolder(
                    new List<PlaceholderNameAndValue>(),
                    new RouteBuilder()
                        .WithDownstreamRoute(downstreamRoute)
                        .WithUpstreamHttpMethod(new List<string> { "Get" })
                        .Build())))
                .And(x => x.GivenTheDownstreamRequestUriIs("http://my.url/abc?q=123"))
                .And(x => GivenTheServiceProviderConfigIs(config))
                .And(x => x.GivenTheUrlReplacerWillReturn("/api/products/1"))
                .When(x => x.WhenICallTheMiddleware())
                .Then(x => x.ThenTheDownstreamRequestUriIs("https://my.url:80/api/products/1?q=123"))
                .And(x => ThenTheQueryStringIs("?q=123"))
                .BDDfy();
        }

        [Fact]
        public void should_replace_query_string()
        {
            var downstreamRoute = new DownstreamRouteBuilder()
                .WithDownstreamPathTemplate("/api/units/{subscriptionId}/{unitId}/updates")
                .WithUpstreamHttpMethod(new List<string> { "Get" })
                .WithDownstreamScheme("https")
                .Build();

            var config = new ServiceProviderConfigurationBuilder()
                .Build();

            this.Given(x => x.GivenTheDownStreamRouteIs(
                    new DownstreamRouteHolder(
                        new List<PlaceholderNameAndValue>
                        {
                            new PlaceholderNameAndValue("{subscriptionId}", "1"),
                            new PlaceholderNameAndValue("{unitId}", "2")
                        },
                        new RouteBuilder()
                            .WithDownstreamRoute(downstreamRoute)
                            .WithUpstreamHttpMethod(new List<string> { "Get" })
                            .Build())))
                .And(x => x.GivenTheDownstreamRequestUriIs("http://localhost:5000/api/subscriptions/1/updates?unitId=2"))
                .And(x => GivenTheServiceProviderConfigIs(config))
                .And(x => x.GivenTheUrlReplacerWillReturn("api/units/1/2/updates"))
                .When(x => x.WhenICallTheMiddleware())
                .Then(x => x.ThenTheDownstreamRequestUriIs("https://localhost:5000/api/units/1/2/updates"))
                .And(x => ThenTheQueryStringIs(""))
                .BDDfy();
        }

        [Fact]
        public void should_replace_query_string_but_leave_non_placeholder_queries()
        {
            var downstreamRoute = new DownstreamRouteBuilder()
                .WithDownstreamPathTemplate("/api/units/{subscriptionId}/{unitId}/updates")
                .WithUpstreamHttpMethod(new List<string> { "Get" })
                .WithDownstreamScheme("https")
                .Build();

            var config = new ServiceProviderConfigurationBuilder()
                .Build();

            this.Given(x => x.GivenTheDownStreamRouteIs(
                    new DownstreamRouteHolder(
                        new List<PlaceholderNameAndValue>
                        {
                            new PlaceholderNameAndValue("{subscriptionId}", "1"),
                            new PlaceholderNameAndValue("{unitId}", "2")
                        },
                        new RouteBuilder()
                            .WithDownstreamRoute(downstreamRoute)
                            .WithUpstreamHttpMethod(new List<string> { "Get" })
                            .Build())))
                .And(x => x.GivenTheDownstreamRequestUriIs("http://localhost:5000/api/subscriptions/1/updates?unitId=2&productId=2"))
                .And(x => GivenTheServiceProviderConfigIs(config))
                .And(x => x.GivenTheUrlReplacerWillReturn("api/units/1/2/updates"))
                .When(x => x.WhenICallTheMiddleware())
                .Then(x => x.ThenTheDownstreamRequestUriIs("https://localhost:5000/api/units/1/2/updates?productId=2"))
                .And(x => ThenTheQueryStringIs("?productId=2"))
                .BDDfy();
        }

        [Fact]
        public void should_replace_query_string_exact_match()
        {
            var downstreamRoute = new DownstreamRouteBuilder()
                .WithDownstreamPathTemplate("/api/units/{subscriptionId}/{unitId}/updates/{unitIdIty}")
                .WithUpstreamHttpMethod(new List<string> { "Get" })
                .WithDownstreamScheme("https")
                .Build();

            var config = new ServiceProviderConfigurationBuilder()
                .Build();

            this.Given(x => x.GivenTheDownStreamRouteIs(
                    new DownstreamRouteHolder(
                        new List<PlaceholderNameAndValue>
                        {
                            new PlaceholderNameAndValue("{subscriptionId}", "1"),
                            new PlaceholderNameAndValue("{unitId}", "2"),
                            new PlaceholderNameAndValue("{unitIdIty}", "3")
                        },
                        new RouteBuilder()
                            .WithDownstreamRoute(downstreamRoute)
                            .WithUpstreamHttpMethod(new List<string> { "Get" })
                            .Build())))
                .And(x => x.GivenTheDownstreamRequestUriIs("http://localhost:5000/api/subscriptions/1/updates?unitId=2?unitIdIty=3"))
                .And(x => GivenTheServiceProviderConfigIs(config))
                .And(x => x.GivenTheUrlReplacerWillReturn("api/units/1/2/updates/3"))
                .When(x => x.WhenICallTheMiddleware())
                .Then(x => x.ThenTheDownstreamRequestUriIs("https://localhost:5000/api/units/1/2/updates/3"))
                .And(x => ThenTheQueryStringIs(""))
                .BDDfy();
        }

        [Fact]
        public void should_not_create_service_fabric_url()
        {
            var downstreamRoute = new DownstreamRouteBuilder()
                .WithDownstreamPathTemplate("any old string")
                .WithUpstreamHttpMethod(new List<string> { "Get" })
                .WithDownstreamScheme("https")
                .Build();

            var config = new ServiceProviderConfigurationBuilder()
                .WithType("ServiceFabric")
                .WithHost("localhost")
                .WithPort(19081)
                .Build();

            this.Given(x => x.GivenTheDownStreamRouteIs(
                    new DownstreamRouteHolder(
                        new List<PlaceholderNameAndValue>(),
                        new RouteBuilder()
                            .WithDownstreamRoute(downstreamRoute)
                            .WithUpstreamHttpMethod(new List<string> { "Get" })
                            .Build())))
                .And(x => x.GivenTheDownstreamRequestUriIs("http://my.url/abc?q=123"))
                .And(x => GivenTheServiceProviderConfigIs(config))
                .And(x => x.GivenTheUrlReplacerWillReturn("/api/products/1"))
                .When(x => x.WhenICallTheMiddleware())
                .Then(x => x.ThenTheDownstreamRequestUriIs("https://my.url:80/api/products/1?q=123"))
                .BDDfy();
        }

        [Fact]
        public void should_create_service_fabric_url()
        {
            var downstreamRoute = new DownstreamRouteBuilder()
                .WithDownstreamScheme("http")
                .WithServiceName("Ocelot/OcelotApp")
                .WithUseServiceDiscovery(true)
                .Build();

            var downstreamRouteHolder = new Ocelot.DownstreamRouteFinder.DownstreamRouteHolder(
                new List<PlaceholderNameAndValue>(),
                new RouteBuilder()
                    .WithDownstreamRoute(downstreamRoute)
                    .Build());

            var config = new ServiceProviderConfigurationBuilder()
                .WithType("ServiceFabric")
                .WithHost("localhost")
                .WithPort(19081)
                .Build();

            this.Given(x => x.GivenTheDownStreamRouteIs(downstreamRouteHolder))
                .And(x => GivenTheServiceProviderConfigIs(config))
                .And(x => x.GivenTheDownstreamRequestUriIs("http://localhost:19081"))
                .And(x => x.GivenTheUrlReplacerWillReturnSequence("/api/products/1", "Ocelot/OcelotApp"))
                .When(x => x.WhenICallTheMiddleware())
                .Then(x => x.ThenTheDownstreamRequestUriIs("http://localhost:19081/Ocelot/OcelotApp/api/products/1"))
                .BDDfy();
        }

        [Fact]
        public void should_create_service_fabric_url_with_query_string_for_stateless_service()
        {
            var downstreamRoute = new DownstreamRouteBuilder()
                .WithDownstreamScheme("http")
                .WithServiceName("Ocelot/OcelotApp")
                .WithUseServiceDiscovery(true)
                .Build();

            var downstreamRouteHolder = new Ocelot.DownstreamRouteFinder.DownstreamRouteHolder(
                new List<PlaceholderNameAndValue>(),
                new RouteBuilder()
                    .WithDownstreamRoute(downstreamRoute)
                    .Build());

            var config = new ServiceProviderConfigurationBuilder()
                .WithType("ServiceFabric")
                .WithHost("localhost")
                .WithPort(19081)
                .Build();

            this.Given(x => x.GivenTheDownStreamRouteIs(downstreamRouteHolder))
                .And(x => GivenTheServiceProviderConfigIs(config))
                .And(x => x.GivenTheDownstreamRequestUriIs("http://localhost:19081?Tom=test&laura=1"))
                .And(x => x.GivenTheUrlReplacerWillReturnSequence("/api/products/1", "Ocelot/OcelotApp"))
                .When(x => x.WhenICallTheMiddleware())
                .Then(x => x.ThenTheDownstreamRequestUriIs("http://localhost:19081/Ocelot/OcelotApp/api/products/1?Tom=test&laura=1"))
                .BDDfy();
        }

        [Fact]
        public void should_create_service_fabric_url_with_query_string_for_stateful_service()
        {
            var downstreamRoute = new DownstreamRouteBuilder()
                .WithDownstreamScheme("http")
                .WithServiceName("Ocelot/OcelotApp")
                .WithUseServiceDiscovery(true)
                .Build();

            var downstreamRouteHolder = new Ocelot.DownstreamRouteFinder.DownstreamRouteHolder(
                new List<PlaceholderNameAndValue>(),
                new RouteBuilder()
                    .WithDownstreamRoute(downstreamRoute)
                    .Build());

            var config = new ServiceProviderConfigurationBuilder()
                .WithType("ServiceFabric")
                .WithHost("localhost")
                .WithPort(19081)
                .Build();

            this.Given(x => x.GivenTheDownStreamRouteIs(downstreamRouteHolder))
                .And(x => GivenTheServiceProviderConfigIs(config))
                .And(x => x.GivenTheDownstreamRequestUriIs("http://localhost:19081?PartitionKind=test&PartitionKey=1"))
                .And(x => x.GivenTheUrlReplacerWillReturnSequence("/api/products/1", "Ocelot/OcelotApp"))
                .When(x => x.WhenICallTheMiddleware())
                .Then(x => x.ThenTheDownstreamRequestUriIs("http://localhost:19081/Ocelot/OcelotApp/api/products/1?PartitionKind=test&PartitionKey=1"))
                .BDDfy();
        }

        [Fact]
        public void should_create_service_fabric_url_with_version_from_upstream_path_template()
        {
            var downstreamRoute = new Ocelot.DownstreamRouteFinder.DownstreamRouteHolder(
                new List<PlaceholderNameAndValue>(),
                new RouteBuilder().WithDownstreamRoute(
                        new DownstreamRouteBuilder()
                            .WithDownstreamScheme("http")
                            .WithUpstreamPathTemplate(new UpstreamPathTemplateBuilder().WithOriginalValue("/products").Build())
                            .WithUseServiceDiscovery(true)
                            .Build()
                    ).Build());

            var config = new ServiceProviderConfigurationBuilder()
                .WithType("ServiceFabric")
                .WithHost("localhost")
                .WithPort(19081)
                .Build();

            this.Given(x => x.GivenTheDownStreamRouteIs(downstreamRoute))
                .And(x => GivenTheServiceProviderConfigIs(config))
                .And(x => x.GivenTheDownstreamRequestUriIs("http://localhost:19081?PartitionKind=test&PartitionKey=1"))
                .And(x => x.GivenTheUrlReplacerWillReturnSequence("/products", "Service_1.0/Api"))
                .When(x => x.WhenICallTheMiddleware())
                .Then(x => x.ThenTheDownstreamRequestUriIs("http://localhost:19081/Service_1.0/Api/products?PartitionKind=test&PartitionKey=1"))
                .BDDfy();
        }

        [Fact]
        public void issue_473_should_not_remove_additional_query_string()
        {
            var downstreamRoute = new DownstreamRouteBuilder()
                .WithDownstreamPathTemplate("/Authorized/{action}?server={server}")
                .WithUpstreamHttpMethod(new List<string> { "Post", "Get" })
                .WithDownstreamScheme("http")
                .WithUpstreamPathTemplate(new UpstreamPathTemplateBuilder().WithOriginalValue("/uc/Authorized/{server}/{action}").Build())
                .Build();

            var config = new ServiceProviderConfigurationBuilder()
                .Build();

            this.Given(x => x.GivenTheDownStreamRouteIs(
                    new DownstreamRouteHolder(
                        new List<PlaceholderNameAndValue>
                        {
                            new PlaceholderNameAndValue("{action}", "1"),
                            new PlaceholderNameAndValue("{server}", "2")
                        },
                        new RouteBuilder()
                            .WithDownstreamRoute(downstreamRoute)
                            .WithUpstreamHttpMethod(new List<string> { "Post", "Get" })
                            .Build())))
                .And(x => x.GivenTheDownstreamRequestUriIs("http://localhost:5000/uc/Authorized/2/1/refresh?refreshToken=2288356cfb1338fdc5ff4ca558ec785118dfe1ff2864340937da8226863ff66d"))
                .And(x => GivenTheServiceProviderConfigIs(config))
                .And(x => x.GivenTheUrlReplacerWillReturn("/Authorized/1?server=2"))
                .When(x => x.WhenICallTheMiddleware())
                .Then(x => x.ThenTheDownstreamRequestUriIs("http://localhost:5000/Authorized/1?refreshToken=2288356cfb1338fdc5ff4ca558ec785118dfe1ff2864340937da8226863ff66d&server=2"))
                .And(x => ThenTheQueryStringIs("?refreshToken=2288356cfb1338fdc5ff4ca558ec785118dfe1ff2864340937da8226863ff66d&server=2"))
                .BDDfy();
        }

        [Fact]
        public void should_not_replace_by_empty_scheme()
        {
            var downstreamRoute = new DownstreamRouteBuilder()
                .WithDownstreamScheme("")
                .WithServiceName("Ocelot/OcelotApp")
                .WithUseServiceDiscovery(true)
                .Build();

            var downstreamRouteHolder = new Ocelot.DownstreamRouteFinder.DownstreamRouteHolder(
                new List<PlaceholderNameAndValue>(),
                new RouteBuilder()
                    .WithDownstreamRoute(downstreamRoute)
                    .Build());

            var config = new ServiceProviderConfigurationBuilder()
                .WithType("ServiceFabric")
                .WithHost("localhost")
                .WithPort(19081)
                .Build();

            this.Given(x => x.GivenTheDownStreamRouteIs(downstreamRouteHolder))
                .And(x => GivenTheServiceProviderConfigIs(config))
                .And(x => x.GivenTheDownstreamRequestUriIs("https://localhost:19081?PartitionKind=test&PartitionKey=1"))
                .And(x => x.GivenTheUrlReplacerWillReturnSequence("/api/products/1", "Ocelot/OcelotApp"))
                .When(x => x.WhenICallTheMiddleware())
                .Then(x => x.ThenTheDownstreamRequestUriIs("https://localhost:19081/Ocelot/OcelotApp/api/products/1?PartitionKind=test&PartitionKey=1"))
                .BDDfy();
        }

        private void GivenTheServiceProviderConfigIs(ServiceProviderConfiguration config)
        {
            var configuration = new InternalConfiguration(null, null, config, null, null, null, null, null, null);
            _httpContext.Items.SetIInternalConfiguration(configuration);
        }

        private void WhenICallTheMiddleware()
        {
            _middleware = new DownstreamUrlCreatorMiddleware(_next, _loggerFactory.Object, _downstreamUrlTemplateVariableReplacer.Object);
            _middleware.Invoke(_httpContext).GetAwaiter().GetResult();
        }

        private void GivenTheDownStreamRouteIs(Ocelot.DownstreamRouteFinder.DownstreamRouteHolder downstreamRoute)
        {
            _httpContext.Items.UpsertTemplatePlaceholderNameAndValues(downstreamRoute.TemplatePlaceholderNameAndValues);

            _httpContext.Items.UpsertDownstreamRoute(downstreamRoute.Route.DownstreamRoute[0]);
        }

        private void GivenTheDownstreamRequestUriIs(string uri)
        {
            _request.RequestUri = new Uri(uri);
            _httpContext.Items.UpsertDownstreamRequest(new DownstreamRequest(_request));
        }

        private void GivenTheUrlReplacerWillReturnSequence(params string[] paths)
        {
            var setup = _downstreamUrlTemplateVariableReplacer
                .SetupSequence(x => x.Replace(It.IsAny<string>(), It.IsAny<List<PlaceholderNameAndValue>>()));
            foreach (var path in paths)
            {
                var response = new OkResponse<DownstreamPath>(new DownstreamPath(path));
                setup.Returns(response);
            }
        }

        private void GivenTheUrlReplacerWillReturn(string path)
        {
            _downstreamPath = new OkResponse<DownstreamPath>(new DownstreamPath(path));
            _downstreamUrlTemplateVariableReplacer
                .Setup(x => x.Replace(It.IsAny<string>(), It.IsAny<List<PlaceholderNameAndValue>>()))
                .Returns(_downstreamPath);
        }

        private void ThenTheDownstreamRequestUriIs(string expectedUri)
        {
            _httpContext.Items.DownstreamRequest().ToHttpRequestMessage().RequestUri.OriginalString.ShouldBe(expectedUri);
        }

        private void ThenTheQueryStringIs(string queryString)
        {
            _httpContext.Items.DownstreamRequest().Query.ShouldBe(queryString);
        }
    }
}
