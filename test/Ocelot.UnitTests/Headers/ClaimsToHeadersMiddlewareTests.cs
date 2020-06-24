namespace Ocelot.UnitTests.Headers
{
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Ocelot.Configuration;
    using Ocelot.Configuration.Builder;
    using Ocelot.DownstreamRouteFinder;
    using Ocelot.DownstreamRouteFinder.Middleware;
    using Ocelot.DownstreamRouteFinder.UrlMatcher;
    using Ocelot.Headers;
    using Ocelot.Headers.Middleware;
    using Ocelot.Logging;
    using Ocelot.Middleware;
    using Ocelot.Request.Middleware;
    using Ocelot.Responses;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using TestStack.BDDfy;
    using Xunit;

    public class ClaimsToHeadersMiddlewareTests
    {
        private readonly Mock<IAddHeadersToRequest> _addHeaders;
        private Response<Ocelot.DownstreamRouteFinder.DownstreamRouteHolder> _downstreamRoute;
        private Mock<IOcelotLoggerFactory> _loggerFactory;
        private Mock<IOcelotLogger> _logger;
        private ClaimsToHeadersMiddleware _middleware;
        private RequestDelegate _next;
        private HttpContext _httpContext;

        public ClaimsToHeadersMiddlewareTests()
        {
            _httpContext = new DefaultHttpContext();
            _addHeaders = new Mock<IAddHeadersToRequest>();
            _loggerFactory = new Mock<IOcelotLoggerFactory>();
            _logger = new Mock<IOcelotLogger>();
            _loggerFactory.Setup(x => x.CreateLogger<ClaimsToHeadersMiddleware>()).Returns(_logger.Object);
            _next = context => Task.CompletedTask;
            _middleware = new ClaimsToHeadersMiddleware(_next, _loggerFactory.Object, _addHeaders.Object);
            _httpContext.Items.UpsertDownstreamRequest(new DownstreamRequest(new HttpRequestMessage(HttpMethod.Get, "http://test.com")));
        }

        [Fact]
        public void should_call_add_headers_to_request_correctly()
        {
            var downstreamRoute = new Ocelot.DownstreamRouteFinder.DownstreamRouteHolder(new List<PlaceholderNameAndValue>(),
                new RouteBuilder()
                    .WithDownstreamRoute(new DownstreamRouteBuilder()
                            .WithDownstreamPathTemplate("any old string")
                            .WithClaimsToHeaders(new List<ClaimToThing>
                            {
                                new ClaimToThing("UserId", "Subject", "", 0)
                            })
                            .WithUpstreamHttpMethod(new List<string> { "Get" })
                            .Build())
                    .WithUpstreamHttpMethod(new List<string> { "Get" })
                    .Build());

            this.Given(x => x.GivenTheDownStreamRouteIs(downstreamRoute))
                .And(x => x.GivenTheAddHeadersToDownstreamRequestReturnsOk())
                .When(x => x.WhenICallTheMiddleware())
                .Then(x => x.ThenTheAddHeadersToRequestIsCalledCorrectly())
                .BDDfy();
        }

        private void WhenICallTheMiddleware()
        {
            _middleware.Invoke(_httpContext).GetAwaiter().GetResult();
        }

        private void GivenTheDownStreamRouteIs(Ocelot.DownstreamRouteFinder.DownstreamRouteHolder downstreamRoute)
        {
            _downstreamRoute = new OkResponse<Ocelot.DownstreamRouteFinder.DownstreamRouteHolder>(downstreamRoute);

            _httpContext.Items.UpsertTemplatePlaceholderNameAndValues(downstreamRoute.TemplatePlaceholderNameAndValues);

            _httpContext.Items.UpsertDownstreamRoute(downstreamRoute.Route.DownstreamRoute[0]);
        }

        private void GivenTheAddHeadersToDownstreamRequestReturnsOk()
        {
            _addHeaders
                .Setup(x => x.SetHeadersOnDownstreamRequest(
                    It.IsAny<List<ClaimToThing>>(),
                    It.IsAny<IEnumerable<System.Security.Claims.Claim>>(),
                    It.IsAny<DownstreamRequest>()))
                .Returns(new OkResponse());
        }

        private void ThenTheAddHeadersToRequestIsCalledCorrectly()
        {
            _addHeaders
                .Verify(x => x.SetHeadersOnDownstreamRequest(
                    It.IsAny<List<ClaimToThing>>(),
                    It.IsAny<IEnumerable<System.Security.Claims.Claim>>(),
                    _httpContext.Items.DownstreamRequest()), Times.Once);
        }
    }
}
