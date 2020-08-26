using Ocelot.Middleware;

namespace Ocelot.UnitTests.Claims
{
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Ocelot.Claims;
    using Ocelot.Claims.Middleware;
    using Ocelot.Configuration;
    using Ocelot.Configuration.Builder;
    using Ocelot.DownstreamRouteFinder;
    using Ocelot.DownstreamRouteFinder.Middleware;
    using Ocelot.DownstreamRouteFinder.UrlMatcher;
    using Ocelot.Logging;
    using Ocelot.Responses;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TestStack.BDDfy;
    using Xunit;

    public class ClaimsToClaimsMiddlewareTests
    {
        private readonly Mock<IAddClaimsToRequest> _addHeaders;
        private Mock<IOcelotLoggerFactory> _loggerFactory;
        private Mock<IOcelotLogger> _logger;
        private readonly ClaimsToClaimsMiddleware _middleware;
        private RequestDelegate _next;
        private HttpContext _httpContext;

        public ClaimsToClaimsMiddlewareTests()
        {
            _httpContext = new DefaultHttpContext();
            _addHeaders = new Mock<IAddClaimsToRequest>();
            _loggerFactory = new Mock<IOcelotLoggerFactory>();
            _logger = new Mock<IOcelotLogger>();
            _loggerFactory.Setup(x => x.CreateLogger<ClaimsToClaimsMiddleware>()).Returns(_logger.Object);
            _next = context => Task.CompletedTask;
            _middleware = new ClaimsToClaimsMiddleware(_next, _loggerFactory.Object, _addHeaders.Object);
        }

        [Fact]
        public void should_call_claims_to_request_correctly()
        {
            var downstreamRoute = new Ocelot.DownstreamRouteFinder.DownstreamRouteHolder(new List<PlaceholderNameAndValue>(),
                new RouteBuilder()
                    .WithDownstreamRoute(new DownstreamRouteBuilder()
                        .WithDownstreamPathTemplate("any old string")
                        .WithClaimsToClaims(new List<ClaimToThing>
                        {
                            new ClaimToThing("sub", "UserType", "|", 0)
                        })
                        .WithUpstreamHttpMethod(new List<string> { "Get" })
                        .Build())
                    .WithUpstreamHttpMethod(new List<string> { "Get" })
                    .Build());

            this.Given(x => x.GivenTheDownStreamRouteIs(downstreamRoute))
                .And(x => x.GivenTheAddClaimsToRequestReturns())
                .When(x => x.WhenICallTheMiddleware())
                .Then(x => x.ThenTheClaimsToRequestIsCalledCorrectly())
                .BDDfy();
        }

        private void WhenICallTheMiddleware()
        {
            _middleware.Invoke(_httpContext).GetAwaiter().GetResult();
        }

        private void GivenTheDownStreamRouteIs(Ocelot.DownstreamRouteFinder.DownstreamRouteHolder downstreamRoute)
        {
            _httpContext.Items.UpsertTemplatePlaceholderNameAndValues(downstreamRoute.TemplatePlaceholderNameAndValues);

            _httpContext.Items.UpsertDownstreamRoute(downstreamRoute.Route.DownstreamRoute[0]);
        }

        private void GivenTheAddClaimsToRequestReturns()
        {
            _addHeaders
                .Setup(x => x.SetClaimsOnContext(It.IsAny<List<ClaimToThing>>(),
                It.IsAny<HttpContext>()))
                .Returns(new OkResponse());
        }

        private void ThenTheClaimsToRequestIsCalledCorrectly()
        {
            _addHeaders
                .Verify(x => x.SetClaimsOnContext(It.IsAny<List<ClaimToThing>>(),
                It.IsAny<HttpContext>()), Times.Once);
        }
    }
}
