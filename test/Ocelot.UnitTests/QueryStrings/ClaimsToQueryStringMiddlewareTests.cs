using Ocelot.Middleware;

namespace Ocelot.UnitTests.QueryStrings
{
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Ocelot.Configuration;
    using Ocelot.Configuration.Builder;
    using Ocelot.DownstreamRouteFinder;
    using Ocelot.DownstreamRouteFinder.UrlMatcher;
    using Ocelot.Logging;
    using Ocelot.QueryStrings;
    using Ocelot.QueryStrings.Middleware;
    using Ocelot.Request.Middleware;
    using Ocelot.Responses;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Ocelot.Infrastructure.RequestData;
    using TestStack.BDDfy;
    using Xunit;
    using Ocelot.DownstreamRouteFinder.Middleware;

    public class ClaimsToQueryStringMiddlewareTests
    {
        private readonly Mock<IAddQueriesToRequest> _addQueries;
        private Mock<IOcelotLoggerFactory> _loggerFactory;
        private Mock<IOcelotLogger> _logger;
        private ClaimsToQueryStringMiddleware _middleware;
        private RequestDelegate _next;
        private HttpContext _httpContext;
        private Mock<IRequestScopedDataRepository> _repo;

        public ClaimsToQueryStringMiddlewareTests()
        {
            _repo = new Mock<IRequestScopedDataRepository>();
            _httpContext = new DefaultHttpContext();
            _loggerFactory = new Mock<IOcelotLoggerFactory>();
            _logger = new Mock<IOcelotLogger>();
            _loggerFactory.Setup(x => x.CreateLogger<ClaimsToQueryStringMiddleware>()).Returns(_logger.Object);
            _next = context => Task.CompletedTask;
            _addQueries = new Mock<IAddQueriesToRequest>();
            _httpContext.Items.UpsertDownstreamRequest(new DownstreamRequest(new HttpRequestMessage(HttpMethod.Get, "http://test.com")));
            _middleware = new ClaimsToQueryStringMiddleware(_next, _loggerFactory.Object, _addQueries.Object);
        }

        [Fact]
        public void should_call_add_queries_correctly()
        {
            var downstreamRoute = new Ocelot.DownstreamRouteFinder.DownstreamRouteHolder(new List<PlaceholderNameAndValue>(),
                new RouteBuilder()
                    .WithDownstreamRoute(new DownstreamRouteBuilder()
                        .WithDownstreamPathTemplate("any old string")
                        .WithClaimsToQueries(new List<ClaimToThing>
                        {
                            new ClaimToThing("UserId", "Subject", "", 0)
                        })
                        .WithUpstreamHttpMethod(new List<string> { "Get" })
                        .Build())
                    .WithUpstreamHttpMethod(new List<string> { "Get" })
                    .Build());

            this.Given(x => x.GivenTheDownStreamRouteIs(downstreamRoute))
                .And(x => x.GivenTheAddHeadersToRequestReturnsOk())
                .When(x => x.WhenICallTheMiddleware())
                .Then(x => x.ThenTheAddQueriesToRequestIsCalledCorrectly())
                .BDDfy();
        }

        private void WhenICallTheMiddleware()
        {
            _middleware.Invoke(_httpContext).GetAwaiter().GetResult();
        }

        private void GivenTheAddHeadersToRequestReturnsOk()
        {
            _addQueries
                .Setup(x => x.SetQueriesOnDownstreamRequest(
                    It.IsAny<List<ClaimToThing>>(),
                    It.IsAny<IEnumerable<Claim>>(),
                    It.IsAny<DownstreamRequest>()))
                .Returns(new OkResponse());
        }

        private void ThenTheAddQueriesToRequestIsCalledCorrectly()
        {
            _addQueries
                .Verify(x => x.SetQueriesOnDownstreamRequest(
                    It.IsAny<List<ClaimToThing>>(),
                    It.IsAny<IEnumerable<Claim>>(),
                    _httpContext.Items.DownstreamRequest()), Times.Once);
        }

        private void GivenTheDownStreamRouteIs(Ocelot.DownstreamRouteFinder.DownstreamRouteHolder downstreamRoute)
        {
            _httpContext.Items.UpsertTemplatePlaceholderNameAndValues(downstreamRoute.TemplatePlaceholderNameAndValues);

            _httpContext.Items.UpsertDownstreamRoute(downstreamRoute.Route.DownstreamRoute[0]);
        }
    }
}
