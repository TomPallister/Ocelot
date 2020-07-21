using Ocelot.Middleware;

namespace Ocelot.UnitTests.Responder
{
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Ocelot.DownstreamRouteFinder.Finder;
    using Ocelot.Errors;
    using Ocelot.Logging;
    using Ocelot.Responder;
    using Ocelot.Responder.Middleware;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Ocelot.Infrastructure.RequestData;
    using TestStack.BDDfy;
    using Xunit;
    using Ocelot.DownstreamRouteFinder.Middleware;

    public class ResponderMiddlewareTests
    {
        private readonly Mock<IHttpResponder> _responder;
        private readonly Mock<IErrorsToHttpStatusCodeMapper> _codeMapper;
        private Mock<IOcelotLoggerFactory> _loggerFactory;
        private Mock<IOcelotLogger> _logger;
        private readonly ResponderMiddleware _middleware;
        private RequestDelegate _next;
        private HttpContext _httpContext;

        public ResponderMiddlewareTests()
        {
            _httpContext = new DefaultHttpContext();
            _responder = new Mock<IHttpResponder>();
            _codeMapper = new Mock<IErrorsToHttpStatusCodeMapper>();
            _loggerFactory = new Mock<IOcelotLoggerFactory>();
            _logger = new Mock<IOcelotLogger>();
            _loggerFactory.Setup(x => x.CreateLogger<ResponderMiddleware>()).Returns(_logger.Object);
            _next = context => Task.CompletedTask;
            _middleware = new ResponderMiddleware(_next, _responder.Object, _loggerFactory.Object, _codeMapper.Object);
        }

        [Fact]
        public void should_not_return_any_errors()
        {
            this.Given(x => x.GivenTheHttpResponseMessageIs(new DownstreamResponse(new HttpResponseMessage())))
                .When(x => x.WhenICallTheMiddleware())
                .Then(x => x.ThenThereAreNoErrors())
                .BDDfy();
        }

        [Fact]
        public void should_return_any_errors()
        {
            this.Given(x => x.GivenTheHttpResponseMessageIs(new DownstreamResponse(new HttpResponseMessage())))
                .And(x => x.GivenThereArePipelineErrors(new UnableToFindDownstreamRouteError("/path", "GET")))
                .When(x => x.WhenICallTheMiddleware())
                .Then(x => x.ThenThereAreNoErrors())
                .BDDfy();
        }

        private void WhenICallTheMiddleware()
        {
            _middleware.Invoke(_httpContext).GetAwaiter().GetResult();
        }

        private void GivenTheHttpResponseMessageIs(DownstreamResponse response)
        {
            _httpContext.Items.UpsertDownstreamResponse(response);
        }

        private void ThenThereAreNoErrors()
        {
            //todo a better assert?
        }

        private void GivenThereArePipelineErrors(Error error)
        {
            _httpContext.Items.SetError(error);
        }
    }
}
