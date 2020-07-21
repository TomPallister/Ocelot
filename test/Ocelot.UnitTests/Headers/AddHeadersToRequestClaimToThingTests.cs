using Moq;
using Ocelot.Configuration;
using Ocelot.Errors;
using Ocelot.Headers;
using Ocelot.Infrastructure.Claims.Parser;
using Ocelot.Request.Middleware;
using Ocelot.Responses;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.Headers
{
    using Ocelot.Infrastructure;
    using Ocelot.Logging;

    public class AddHeadersToRequestClaimToThingTests
    {
        private readonly AddHeadersToRequest _addHeadersToRequest;
        private readonly Mock<IClaimsParser> _parser;
        private readonly DownstreamRequest _downstreamRequest;
        private List<Claim> _claims;
        private List<ClaimToThing> _configuration;
        private Response _result;
        private Response<string> _claimValue;
        private Mock<IPlaceholders> _placeholders;
        private Mock<IOcelotLoggerFactory> _factory;

        public AddHeadersToRequestClaimToThingTests()
        {
            _parser = new Mock<IClaimsParser>();
            _placeholders = new Mock<IPlaceholders>();
            _factory = new Mock<IOcelotLoggerFactory>();
            _addHeadersToRequest = new AddHeadersToRequest(_parser.Object, _placeholders.Object, _factory.Object);
            _downstreamRequest = new DownstreamRequest(new HttpRequestMessage(HttpMethod.Get, "http://test.com"));
        }

        [Fact]
        public void should_add_headers_to_downstreamRequest()
        {
            var claims = new List<Claim>
            {
                new Claim("test", "data")
            };

            this.Given(
                x => x.GivenConfigurationHeaderExtractorProperties(new List<ClaimToThing>
                {
                    new ClaimToThing("header-key", "", "", 0)
                }))
                .Given(x => x.GivenClaims(claims))
                .And(x => x.GivenTheClaimParserReturns(new OkResponse<string>("value")))
                .When(x => x.WhenIAddHeadersToTheRequest())
                .Then(x => x.ThenTheResultIsSuccess())
                .And(x => x.ThenTheHeaderIsAdded())
                .BDDfy();
        }

        [Fact]
        public void should_replace_existing_headers_on_request()
        {
            this.Given(
                x => x.GivenConfigurationHeaderExtractorProperties(new List<ClaimToThing>
                {
                    new ClaimToThing("header-key", "", "", 0)
                }))
                .Given(x => x.GivenClaims(new List<Claim>
                {
                    new Claim("test", "data")
                }))
                .And(x => x.GivenTheClaimParserReturns(new OkResponse<string>("value")))
                .And(x => x.GivenThatTheRequestContainsHeader("header-key", "initial"))
                .When(x => x.WhenIAddHeadersToTheRequest())
                .Then(x => x.ThenTheResultIsSuccess())
                .And(x => x.ThenTheHeaderIsAdded())
                .BDDfy();
        }

        [Fact]
        public void should_return_error()
        {
            this.Given(
               x => x.GivenConfigurationHeaderExtractorProperties(new List<ClaimToThing>
               {
                    new ClaimToThing("", "", "", 0)
               }))
               .Given(x => x.GivenClaims(new List<Claim>()))
               .And(x => x.GivenTheClaimParserReturns(new ErrorResponse<string>(new List<Error>
               {
                   new AnyError()
               })))
               .When(x => x.WhenIAddHeadersToTheRequest())
               .Then(x => x.ThenTheResultIsError())
               .BDDfy();
        }

        private void GivenClaims(List<Claim> claims)
        {
            _claims = claims;
        }

        private void GivenConfigurationHeaderExtractorProperties(List<ClaimToThing> configuration)
        {
            _configuration = configuration;
        }

        private void GivenThatTheRequestContainsHeader(string key, string value)
        {
            _downstreamRequest.Headers.Add(key, value);
        }

        private void GivenTheClaimParserReturns(Response<string> claimValue)
        {
            _claimValue = claimValue;
            _parser
                .Setup(
                    x =>
                        x.GetValue(It.IsAny<IEnumerable<Claim>>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<int>()))
                .Returns(_claimValue);
        }

        private void WhenIAddHeadersToTheRequest()
        {
            _result = _addHeadersToRequest.SetHeadersOnDownstreamRequest(_configuration, _claims, _downstreamRequest);
        }

        private void ThenTheResultIsSuccess()
        {
            _result.IsError.ShouldBe(false);
        }

        private void ThenTheResultIsError()
        {
            _result.IsError.ShouldBe(true);
        }

        private void ThenTheHeaderIsAdded()
        {
            var header = _downstreamRequest.Headers.First(x => x.Key == "header-key");
            header.Value.First().ShouldBe(_claimValue.Data);
        }

        private class AnyError : Error
        {
            public AnyError()
                : base("blahh", OcelotErrorCode.UnknownError, 404)
            {
            }
        }
    }
}
