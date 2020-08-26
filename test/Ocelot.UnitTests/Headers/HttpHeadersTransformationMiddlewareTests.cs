namespace Ocelot.UnitTests.Headers
{
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Ocelot.Authorisation.Middleware;
    using Ocelot.Configuration;
    using Ocelot.Configuration.Builder;
    using Ocelot.DownstreamRouteFinder;
    using Ocelot.Headers;
    using Ocelot.Headers.Middleware;
    using Ocelot.Logging;
    using Ocelot.Middleware;
    using Ocelot.Request.Middleware;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Ocelot.Infrastructure.RequestData;
    using TestStack.BDDfy;
    using Xunit;
    using Ocelot.DownstreamRouteFinder.Middleware;

    public class HttpHeadersTransformationMiddlewareTests
    {
        private readonly Mock<IHttpContextRequestHeaderReplacer> _preReplacer;
        private readonly Mock<IHttpResponseHeaderReplacer> _postReplacer;
        private Mock<IOcelotLoggerFactory> _loggerFactory;
        private Mock<IOcelotLogger> _logger;
        private readonly HttpHeadersTransformationMiddleware _middleware;
        private RequestDelegate _next;
        private readonly Mock<IAddHeadersToResponse> _addHeadersToResponse;
        private readonly Mock<IAddHeadersToRequest> _addHeadersToRequest;
        private HttpContext _httpContext;

        public HttpHeadersTransformationMiddlewareTests()
        {
            _httpContext = new DefaultHttpContext();
            _preReplacer = new Mock<IHttpContextRequestHeaderReplacer>();
            _postReplacer = new Mock<IHttpResponseHeaderReplacer>();
            _loggerFactory = new Mock<IOcelotLoggerFactory>();
            _logger = new Mock<IOcelotLogger>();
            _loggerFactory.Setup(x => x.CreateLogger<AuthorisationMiddleware>()).Returns(_logger.Object);
            _next = context => Task.CompletedTask;
            _addHeadersToResponse = new Mock<IAddHeadersToResponse>();
            _addHeadersToRequest = new Mock<IAddHeadersToRequest>();
            _middleware = new HttpHeadersTransformationMiddleware(
                _next, _loggerFactory.Object, _preReplacer.Object,
                _postReplacer.Object, _addHeadersToResponse.Object, _addHeadersToRequest.Object);
        }

        [Fact]
        public void should_call_pre_and_post_header_transforms()
        {
            this.Given(x => GivenTheFollowingRequest())
                .And(x => GivenTheDownstreamRequestIs())
                .And(x => GivenTheRouteHasPreFindAndReplaceSetUp())
                .And(x => GivenTheHttpResponseMessageIs())
                .When(x => WhenICallTheMiddleware())
                .Then(x => ThenTheIHttpContextRequestHeaderReplacerIsCalledCorrectly())
                .Then(x => ThenAddHeadersToRequestIsCalledCorrectly())
                .And(x => ThenTheIHttpResponseHeaderReplacerIsCalledCorrectly())
                .And(x => ThenAddHeadersToResponseIsCalledCorrectly())
                .BDDfy();
        }

        private void ThenAddHeadersToResponseIsCalledCorrectly()
        {
            _addHeadersToResponse
                .Verify(x => x.Add(_httpContext.Items.DownstreamRoute().AddHeadersToDownstream, _httpContext.Items.DownstreamResponse()), Times.Once);
        }

        private void ThenAddHeadersToRequestIsCalledCorrectly()
        {
            _addHeadersToRequest
                .Verify(x => x.SetHeadersOnDownstreamRequest(_httpContext.Items.DownstreamRoute().AddHeadersToUpstream, _httpContext), Times.Once);
        }

        private void WhenICallTheMiddleware()
        {
            _middleware.Invoke(_httpContext).GetAwaiter().GetResult();
        }

        private void GivenTheDownstreamRequestIs()
        {
            _httpContext.Items.UpsertDownstreamRequest(new DownstreamRequest(new HttpRequestMessage(HttpMethod.Get, "http://test.com")));
        }

        private void GivenTheHttpResponseMessageIs()
        {
            _httpContext.Items.UpsertDownstreamResponse(new DownstreamResponse(new HttpResponseMessage()));
        }

        private void GivenTheRouteHasPreFindAndReplaceSetUp()
        {
            var fAndRs = new List<HeaderFindAndReplace>();
            var route = new RouteBuilder()
                .WithDownstreamRoute(new DownstreamRouteBuilder().WithUpstreamHeaderFindAndReplace(fAndRs)
                    .WithDownstreamHeaderFindAndReplace(fAndRs).Build())
                .Build();

            var dR = new Ocelot.DownstreamRouteFinder.DownstreamRouteHolder(null, route);

            _httpContext.Items.UpsertTemplatePlaceholderNameAndValues(dR.TemplatePlaceholderNameAndValues);
            _httpContext.Items.UpsertDownstreamRoute(dR.Route.DownstreamRoute[0]);
        }

        private void ThenTheIHttpContextRequestHeaderReplacerIsCalledCorrectly()
        {
            _preReplacer.Verify(x => x.Replace(It.IsAny<HttpContext>(), It.IsAny<List<HeaderFindAndReplace>>()), Times.Once);
        }

        private void ThenTheIHttpResponseHeaderReplacerIsCalledCorrectly()
        {
            _postReplacer.Verify(x => x.Replace(It.IsAny<HttpContext>(), It.IsAny<List<HeaderFindAndReplace>>()), Times.Once);
        }

        private void GivenTheFollowingRequest()
        {
            _httpContext.Request.Headers.Add("test", "test");
        }
    }
}
