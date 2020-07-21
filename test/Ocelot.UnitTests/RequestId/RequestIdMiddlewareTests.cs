namespace Ocelot.UnitTests.RequestId
{
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Ocelot.Configuration.Builder;
    using Ocelot.DownstreamRouteFinder;
    using Ocelot.DownstreamRouteFinder.Middleware;
    using Ocelot.DownstreamRouteFinder.UrlMatcher;
    using Ocelot.Infrastructure.RequestData;
    using Ocelot.Logging;
    using Ocelot.Middleware;
    using Ocelot.Request.Middleware;
    using Ocelot.RequestId.Middleware;
    using Ocelot.Responses;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using TestStack.BDDfy;
    using Xunit;

    public class RequestIdMiddlewareTests
    {
        private readonly HttpRequestMessage _downstreamRequest;
        private string _value;
        private string _key;
        private Mock<IOcelotLoggerFactory> _loggerFactory;
        private Mock<IOcelotLogger> _logger;
        private readonly RequestIdMiddleware _middleware;
        private RequestDelegate _next;
        private readonly Mock<IRequestScopedDataRepository> _repo;
        private HttpContext _httpContext;
        public RequestIdMiddlewareTests()
        {
            _httpContext = new DefaultHttpContext();
            _downstreamRequest = new HttpRequestMessage(HttpMethod.Get, "http://test.com");
            _repo = new Mock<IRequestScopedDataRepository>();
            _loggerFactory = new Mock<IOcelotLoggerFactory>();
            _logger = new Mock<IOcelotLogger>();
            _loggerFactory.Setup(x => x.CreateLogger<RequestIdMiddleware>()).Returns(_logger.Object);
            _next = context =>
            {
                _httpContext.Response.Headers.Add("LSRequestId", _httpContext.TraceIdentifier);
                return Task.CompletedTask;
            };
            _middleware = new RequestIdMiddleware(_next, _loggerFactory.Object, _repo.Object);
            _httpContext.Items.UpsertDownstreamRequest(new DownstreamRequest(_downstreamRequest));
        }

        [Fact]
        public void should_pass_down_request_id_from_upstream_request()
        {
            var downstreamRoute = new DownstreamRouteHolder(new List<PlaceholderNameAndValue>(),
                new RouteBuilder()
                    .WithDownstreamRoute(new DownstreamRouteBuilder()
                        .WithDownstreamPathTemplate("any old string")
                        .WithRequestIdKey("LSRequestId")
                        .WithUpstreamHttpMethod(new List<string> { "Get" })
                        .Build())
                .WithUpstreamHttpMethod(new List<string> { "Get" })
                .Build());

            var requestId = Guid.NewGuid().ToString();

            this.Given(x => x.GivenTheDownStreamRouteIs(downstreamRoute))
                .And(x => GivenThereIsNoGlobalRequestId())
                .And(x => x.GivenTheRequestIdIsAddedToTheRequest("LSRequestId", requestId))
                .When(x => x.WhenICallTheMiddleware())
                .Then(x => x.ThenTheTraceIdIs(requestId))
                .BDDfy();
        }

        [Fact]
        public void should_add_request_id_when_not_on_upstream_request()
        {
            var downstreamRoute = new DownstreamRouteHolder(new List<PlaceholderNameAndValue>(),
                new RouteBuilder()
                    .WithDownstreamRoute(new DownstreamRouteBuilder()
                        .WithDownstreamPathTemplate("any old string")
                        .WithRequestIdKey("LSRequestId")
                        .WithUpstreamHttpMethod(new List<string> { "Get" })
                        .Build())
                    .WithUpstreamHttpMethod(new List<string> { "Get" })
                    .Build());

            this.Given(x => x.GivenTheDownStreamRouteIs(downstreamRoute))
                .And(x => GivenThereIsNoGlobalRequestId())
                .When(x => x.WhenICallTheMiddleware())
                .Then(x => x.ThenTheTraceIdIsAnything())
                .BDDfy();
        }

        [Fact]
        public void should_add_request_id_scoped_repo_for_logging_later()
        {
            var downstreamRoute = new DownstreamRouteHolder(new List<PlaceholderNameAndValue>(),
                new RouteBuilder()
                    .WithDownstreamRoute(new DownstreamRouteBuilder()
                        .WithDownstreamPathTemplate("any old string")
                        .WithRequestIdKey("LSRequestId")
                        .WithUpstreamHttpMethod(new List<string> { "Get" })
                        .Build())
                    .WithUpstreamHttpMethod(new List<string> { "Get" })
                    .Build());

            var requestId = Guid.NewGuid().ToString();

            this.Given(x => x.GivenTheDownStreamRouteIs(downstreamRoute))
                .And(x => GivenThereIsNoGlobalRequestId())
                .And(x => x.GivenTheRequestIdIsAddedToTheRequest("LSRequestId", requestId))
                .When(x => x.WhenICallTheMiddleware())
                .Then(x => x.ThenTheTraceIdIs(requestId))
                .And(x => ThenTheRequestIdIsSaved())
                .BDDfy();
        }

        [Fact]
        public void should_update_request_id_scoped_repo_for_logging_later()
        {
            var downstreamRoute = new DownstreamRouteHolder(new List<PlaceholderNameAndValue>(),
                new RouteBuilder()
                    .WithDownstreamRoute(new DownstreamRouteBuilder()
                        .WithDownstreamPathTemplate("any old string")
                        .WithRequestIdKey("LSRequestId")
                        .WithUpstreamHttpMethod(new List<string> { "Get" })
                        .Build())
                    .WithUpstreamHttpMethod(new List<string> { "Get" })
                    .Build());

            var requestId = Guid.NewGuid().ToString();

            this.Given(x => x.GivenTheDownStreamRouteIs(downstreamRoute))
                .And(x => GivenTheRequestIdWasSetGlobally())
                .And(x => x.GivenTheRequestIdIsAddedToTheRequest("LSRequestId", requestId))
                .When(x => x.WhenICallTheMiddleware())
                .Then(x => x.ThenTheTraceIdIs(requestId))
                .And(x => ThenTheRequestIdIsUpdated())
                .BDDfy();
        }

        [Fact]
        public void should_not_update_if_global_request_id_is_same_as_re_route_request_id()
        {
            var downstreamRoute = new DownstreamRouteHolder(new List<PlaceholderNameAndValue>(),
                new RouteBuilder()
                    .WithDownstreamRoute(new DownstreamRouteBuilder()
                        .WithDownstreamPathTemplate("any old string")
                        .WithRequestIdKey("LSRequestId")
                        .WithUpstreamHttpMethod(new List<string> { "Get" })
                        .Build())
                    .WithUpstreamHttpMethod(new List<string> { "Get" })
                    .Build());

            var requestId = "alreadyset";

            this.Given(x => x.GivenTheDownStreamRouteIs(downstreamRoute))
                .And(x => GivenTheRequestIdWasSetGlobally())
                .And(x => x.GivenTheRequestIdIsAddedToTheRequest("LSRequestId", requestId))
                .When(x => x.WhenICallTheMiddleware())
                .Then(x => x.ThenTheTraceIdIs(requestId))
                .And(x => ThenTheRequestIdIsNotUpdated())
                .BDDfy();
        }

        private void WhenICallTheMiddleware()
        {
            _middleware.Invoke(_httpContext).GetAwaiter().GetResult();
        }

        private void GivenThereIsNoGlobalRequestId()
        {
            _repo.Setup(x => x.Get<string>("RequestId")).Returns(new OkResponse<string>(null));
        }

        private void GivenTheRequestIdWasSetGlobally()
        {
            _repo.Setup(x => x.Get<string>("RequestId")).Returns(new OkResponse<string>("alreadyset"));
        }

        private void ThenTheRequestIdIsSaved()
        {
            _repo.Verify(x => x.Add("RequestId", _value), Times.Once);
        }

        private void ThenTheRequestIdIsUpdated()
        {
            _repo.Verify(x => x.Update("RequestId", _value), Times.Once);
        }

        private void ThenTheRequestIdIsNotUpdated()
        {
            _repo.Verify(x => x.Update("RequestId", _value), Times.Never);
        }

        private void GivenTheDownStreamRouteIs(DownstreamRouteHolder downstreamRoute)
        {
            _httpContext.Items.UpsertTemplatePlaceholderNameAndValues(downstreamRoute.TemplatePlaceholderNameAndValues);

            _httpContext.Items.UpsertDownstreamRoute(downstreamRoute.Route.DownstreamRoute[0]);
        }

        private void GivenTheRequestIdIsAddedToTheRequest(string key, string value)
        {
            _key = key;
            _value = value;
            _httpContext.Request.Headers.TryAdd(_key, _value);
        }

        private void ThenTheTraceIdIsAnything()
        {
            _httpContext.Response.Headers.TryGetValue("LSRequestId", out var value);
            value.First().ShouldNotBeNullOrEmpty();
        }

        private void ThenTheTraceIdIs(string expected)
        {
            _httpContext.Response.Headers.TryGetValue("LSRequestId", out var value);
            value.First().ShouldBe(expected);
        }
    }
}
