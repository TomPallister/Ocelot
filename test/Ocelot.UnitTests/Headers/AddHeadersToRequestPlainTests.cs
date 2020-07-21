namespace Ocelot.UnitTests.Headers
{
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Ocelot.Configuration.Creator;
    using Ocelot.Headers;
    using Ocelot.Infrastructure;
    using Ocelot.Infrastructure.Claims.Parser;
    using Ocelot.Logging;
    using Responder;
    using Responses;
    using Shouldly;
    using TestStack.BDDfy;
    using Xunit;

    public class AddHeadersToRequestPlainTests
    {
        private readonly AddHeadersToRequest _addHeadersToRequest;
        private HttpContext _context;
        private AddHeader _addedHeader;
        private readonly Mock<IPlaceholders> _placeholders;
        private Mock<IOcelotLoggerFactory> _factory;
        private readonly Mock<IOcelotLogger> _logger;

        public AddHeadersToRequestPlainTests()
        {
            _placeholders = new Mock<IPlaceholders>();
            _factory = new Mock<IOcelotLoggerFactory>();
            _logger = new Mock<IOcelotLogger>();
            _factory.Setup(x => x.CreateLogger<AddHeadersToRequest>()).Returns(_logger.Object);
            _addHeadersToRequest = new AddHeadersToRequest(Mock.Of<IClaimsParser>(), _placeholders.Object, _factory.Object);
        }

        [Fact]
        public void should_log_error_if_cannot_find_placeholder()
        {
            _placeholders.Setup(x => x.Get(It.IsAny<string>())).Returns(new ErrorResponse<string>(new AnyError()));

            this.Given(_ => GivenHttpRequestWithoutHeaders())
                .When(_ => WhenAddingHeader("X-Forwarded-For", "{RemoteIdAddress}"))
                .Then(_ => ThenAnErrorIsLogged("X-Forwarded-For", "{RemoteIdAddress}"))
                .BDDfy();
        }

        [Fact]
        public void should_add_placeholder_to_downstream_request()
        {
            _placeholders.Setup(x => x.Get(It.IsAny<string>())).Returns(new OkResponse<string>("replaced"));

            this.Given(_ => GivenHttpRequestWithoutHeaders())
                .When(_ => WhenAddingHeader("X-Forwarded-For", "{RemoteIdAddress}"))
                .Then(_ => ThenTheHeaderGetsTakenOverToTheRequestHeaders("replaced"))
                .BDDfy();
        }

        [Fact]
        public void should_add_plain_text_header_to_downstream_request()
        {
            this.Given(_ => GivenHttpRequestWithoutHeaders())
                .When(_ => WhenAddingHeader("X-Custom-Header", "PlainValue"))
                .Then(_ => ThenTheHeaderGetsTakenOverToTheRequestHeaders())
                .BDDfy();
        }

        [Fact]
        public void should_overwrite_existing_header_with_added_header()
        {
            this.Given(_ => GivenHttpRequestWithHeader("X-Custom-Header", "This should get overwritten"))
                .When(_ => WhenAddingHeader("X-Custom-Header", "PlainValue"))
                .Then(_ => ThenTheHeaderGetsTakenOverToTheRequestHeaders())
                .BDDfy();
        }

        private void ThenAnErrorIsLogged(string key, string value)
        {
            _logger.Verify(x => x.LogWarning($"Unable to add header to response {key}: {value}"), Times.Once);
        }

        private void GivenHttpRequestWithoutHeaders()
        {
            _context = new DefaultHttpContext
            {
                Request =
                {
                    Headers =
                    {
                    }
                }
            };
        }

        private void GivenHttpRequestWithHeader(string headerKey, string headerValue)
        {
            _context = new DefaultHttpContext
            {
                Request =
                {
                    Headers =
                    {
                        { headerKey, headerValue }
                    }
                }
            };
        }

        private void WhenAddingHeader(string headerKey, string headerValue)
        {
            _addedHeader = new AddHeader(headerKey, headerValue);
            _addHeadersToRequest.SetHeadersOnDownstreamRequest(new[] { _addedHeader }, _context);
        }

        private void ThenTheHeaderGetsTakenOverToTheRequestHeaders()
        {
            var requestHeaders = _context.Request.Headers;
            requestHeaders.ContainsKey(_addedHeader.Key).ShouldBeTrue($"Header {_addedHeader.Key} was expected but not there.");
            var value = requestHeaders[_addedHeader.Key];
            value.ShouldNotBeNull($"Value of header {_addedHeader.Key} was expected to not be null.");
            value.ToString().ShouldBe(_addedHeader.Value);
        }

        private void ThenTheHeaderGetsTakenOverToTheRequestHeaders(string expected)
        {
            var requestHeaders = _context.Request.Headers;
            var value = requestHeaders[_addedHeader.Key];
            value.ToString().ShouldBe(expected);
        }
    }
}
