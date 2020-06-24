namespace Ocelot.UnitTests.Requester
{
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Ocelot.Configuration.Builder;
    using Ocelot.Logging;
    using Ocelot.Middleware;
    using Ocelot.Requester;
    using Ocelot.Requester.Middleware;
    using Ocelot.Responses;
    using Ocelot.UnitTests.Responder;
    using Shouldly;
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Ocelot.Configuration;
    using Ocelot.Infrastructure.RequestData;
    using TestStack.BDDfy;
    using Xunit;
    using Ocelot.DownstreamRouteFinder.Middleware;

    public class HttpRequesterMiddlewareTests
    {
        private readonly Mock<IHttpRequester> _requester;
        private Response<HttpResponseMessage> _response;
        private Mock<IOcelotLoggerFactory> _loggerFactory;
        private Mock<IOcelotLogger> _logger;
        private readonly HttpRequesterMiddleware _middleware;
        private RequestDelegate _next;
        private HttpContext _httpContext;

        public HttpRequesterMiddlewareTests()
        {
            _httpContext = new DefaultHttpContext();
            _requester = new Mock<IHttpRequester>();
            _loggerFactory = new Mock<IOcelotLoggerFactory>();
            _logger = new Mock<IOcelotLogger>();
            _loggerFactory.Setup(x => x.CreateLogger<HttpRequesterMiddleware>()).Returns(_logger.Object);
            _next = context => Task.CompletedTask;
            _middleware = new HttpRequesterMiddleware(_next, _loggerFactory.Object, _requester.Object);
        }

        [Fact]
        public void should_call_services_correctly()
        {
            this.Given(x => x.GivenTheRequestIs())
                .And(x => x.GivenTheRequesterReturns(new OkResponse<HttpResponseMessage>(new HttpResponseMessage(System.Net.HttpStatusCode.OK))))
                .When(x => x.WhenICallTheMiddleware())
                .Then(x => x.ThenTheDownstreamResponseIsSet())
                .Then(x => InformationIsLogged())
                .BDDfy();
        }

        [Fact]
        public void should_set_error()
        {
            this.Given(x => x.GivenTheRequestIs())
                .And(x => x.GivenTheRequesterReturns(new ErrorResponse<HttpResponseMessage>(new AnyError())))
                .When(x => x.WhenICallTheMiddleware())
                .Then(x => x.ThenTheErrorIsSet())
                .BDDfy();
        }

        [Fact]
        public void should_log_downstream_internal_server_error()
        {
            this.Given(x => x.GivenTheRequestIs())
                    .And(x => x.GivenTheRequesterReturns(
                        new OkResponse<HttpResponseMessage>(new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError))))
                .When(x => x.WhenICallTheMiddleware())
                .Then(x => x.WarningIsLogged())
                .BDDfy();
        }

        private void ThenTheErrorIsSet()
        {
            _httpContext.Items.Errors().Count.ShouldBeGreaterThan(0);
        }

        private void WhenICallTheMiddleware()
        {
            _middleware.Invoke(_httpContext).GetAwaiter().GetResult();
        }

        private void GivenTheRequestIs()
        {
            _httpContext.Items.UpsertDownstreamRoute(new DownstreamRouteBuilder().Build());
        }

        private void GivenTheRequesterReturns(Response<HttpResponseMessage> response)
        {
            _response = response;

            _requester
                .Setup(x => x.GetResponse(It.IsAny<HttpContext>()))
                .ReturnsAsync(_response);
        }

        private void ThenTheDownstreamResponseIsSet()
        {
            foreach (var httpResponseHeader in _response.Data.Headers)
            {
                if (_httpContext.Items.DownstreamResponse().Headers.Any(x => x.Key == httpResponseHeader.Key))
                {
                    throw new Exception("Header in response not in downstreamresponse headers");
                }
            }

            _httpContext.Items.DownstreamResponse().Content.ShouldBe(_response.Data.Content);
            _httpContext.Items.DownstreamResponse().StatusCode.ShouldBe(_response.Data.StatusCode);
        }

        private void WarningIsLogged()
        {
            _logger.Verify(
                x => x.LogWarning(                 
                    It.IsAny<string>()
                   ),
                Times.Once);
        }

        private void InformationIsLogged()
        {
            _logger.Verify(
                x => x.LogInformation(
                    It.IsAny<string>()
                ),
                Times.Once);
        }
    }
}
