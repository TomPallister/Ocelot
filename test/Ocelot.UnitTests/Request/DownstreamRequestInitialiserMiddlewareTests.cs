namespace Ocelot.UnitTests.Request
{
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Ocelot.Infrastructure;
    using Ocelot.Logging;
    using Ocelot.Request.Creator;
    using Ocelot.Request.Mapper;
    using Ocelot.Request.Middleware;
    using Ocelot.Configuration.Builder;
    using Ocelot.Middleware;
    using Ocelot.Responses;
    using Shouldly;
    using System.Net.Http;
    using Ocelot.Configuration;
    using TestStack.BDDfy;
    using Xunit;
    using Ocelot.DownstreamRouteFinder.Middleware;

    public class DownstreamRequestInitialiserMiddlewareTests
    {
        private readonly DownstreamRequestInitialiserMiddleware _middleware;
        private readonly HttpContext _httpContext;
        private readonly Mock<RequestDelegate> _next;
        private readonly Mock<IRequestMapper> _requestMapper;
        private readonly Mock<IOcelotLoggerFactory> _loggerFactory;
        private readonly Mock<IOcelotLogger> _logger;
        private Response<HttpRequestMessage> _mappedRequest;

        public DownstreamRequestInitialiserMiddlewareTests()
        {
            _httpContext = new DefaultHttpContext();
            _requestMapper = new Mock<IRequestMapper>();
            _next = new Mock<RequestDelegate>();
            _logger = new Mock<IOcelotLogger>();

            _loggerFactory = new Mock<IOcelotLoggerFactory>();
            _loggerFactory
                .Setup(lf => lf.CreateLogger<DownstreamRequestInitialiserMiddleware>())
                .Returns(_logger.Object);

            _middleware = new DownstreamRequestInitialiserMiddleware(
                _next.Object,
                _loggerFactory.Object,
                _requestMapper.Object,
                new DownstreamRequestCreator(new FrameworkDescription()));
        }

        [Fact]
        public void Should_handle_valid_httpRequest()
        {
            this.Given(_ => GivenTheHttpContextContainsARequest())
                .And(_ => GivenTheMapperWillReturnAMappedRequest())
                .When(_ => WhenTheMiddlewareIsInvoked())
                .Then(_ => ThenTheContexRequestIsMappedToADownstreamRequest())
                .And(_ => ThenTheDownstreamRequestIsStored())
                .And(_ => ThenTheNextMiddlewareIsInvoked())
                .And(_ => ThenTheDownstreamRequestMethodIs("GET"))
                .BDDfy();
        }

        [Fact]
        public void Should_map_downstream_route_method_to_downstream_request()
        {
            this.Given(_ => GivenTheHttpContextContainsARequest())
                .And(_ => GivenTheMapperWillReturnAMappedRequest())
                .When(_ => WhenTheMiddlewareIsInvoked())
                .Then(_ => ThenTheContexRequestIsMappedToADownstreamRequest())
                .And(_ => ThenTheDownstreamRequestIsStored())
                .And(_ => ThenTheNextMiddlewareIsInvoked())
                .And(_ => ThenTheDownstreamRequestMethodIs("GET"))
                .BDDfy();
        }

        [Fact]
        public void Should_handle_mapping_failure()
        {
            this.Given(_ => GivenTheHttpContextContainsARequest())
                .And(_ => GivenTheMapperWillReturnAnError())
                .When(_ => WhenTheMiddlewareIsInvoked())
                .And(_ => ThenTheDownstreamRequestIsNotStored())
                .And(_ => ThenAPipelineErrorIsStored())
                .And(_ => ThenTheNextMiddlewareIsNotInvoked())
                .BDDfy();
        }

        private void ThenTheDownstreamRequestMethodIs(string expected)
        {
            _httpContext.Items.DownstreamRequest().Method.ShouldBe(expected);
        }

        private void GivenTheHttpContextContainsARequest()
        {
            _httpContext.Items.UpsertDownstreamRoute(new DownstreamRouteBuilder().Build());
        }

        private void GivenTheMapperWillReturnAMappedRequest()
        {
            _mappedRequest = new OkResponse<HttpRequestMessage>(new HttpRequestMessage(HttpMethod.Get, "http://www.bbc.co.uk"));

            _requestMapper
                .Setup(rm => rm.Map(It.IsAny<HttpRequest>(), It.IsAny<DownstreamRoute>()))
                .ReturnsAsync(_mappedRequest);
        }

        private void GivenTheMapperWillReturnAnError()
        {
            _mappedRequest = new ErrorResponse<HttpRequestMessage>(new UnmappableRequestError(new System.Exception("boooom!")));

            _requestMapper
                .Setup(rm => rm.Map(It.IsAny<HttpRequest>(), It.IsAny<DownstreamRoute>()))
                .ReturnsAsync(_mappedRequest);
        }

        private void WhenTheMiddlewareIsInvoked()
        {
            _middleware.Invoke(_httpContext).GetAwaiter().GetResult();
        }

        private void ThenTheContexRequestIsMappedToADownstreamRequest()
        {
            _requestMapper.Verify(rm => rm.Map(_httpContext.Request, _httpContext.Items.DownstreamRoute()), Times.Once);
        }

        private void ThenTheDownstreamRequestIsStored()
        {
            _httpContext.Items.DownstreamRequest().ShouldNotBeNull();
        }

        private void ThenTheDownstreamRequestIsNotStored()
        {
            _httpContext.Items.DownstreamRequest().ShouldBeNull();
        }

        private void ThenAPipelineErrorIsStored()
        {
            _httpContext.Items.Errors().Count.ShouldBeGreaterThan(0);
            _httpContext.Items.Errors().ShouldBe(_mappedRequest.Errors);
        }

        private void ThenTheNextMiddlewareIsInvoked()
        {
            _next.Verify(n => n(_httpContext), Times.Once);
        }

        private void ThenTheNextMiddlewareIsNotInvoked()
        {
            _next.Verify(n => n(It.IsAny<HttpContext>()), Times.Never);
        }
    }
}
