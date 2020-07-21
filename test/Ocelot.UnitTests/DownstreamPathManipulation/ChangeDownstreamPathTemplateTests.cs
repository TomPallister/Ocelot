using Moq;
using Ocelot.Configuration;
using Ocelot.DownstreamRouteFinder.UrlMatcher;
using Ocelot.Errors;
using Ocelot.Infrastructure;
using Ocelot.Infrastructure.Claims.Parser;
using Ocelot.PathManipulation;
using Ocelot.Responses;
using Ocelot.UnitTests.Responder;
using Ocelot.Values;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.DownstreamPathManipulation
{
    public class ChangeDownstreamPathTemplateTests
    {
        private readonly ChangeDownstreamPathTemplate _changeDownstreamPath;
        private DownstreamPathTemplate _downstreamPathTemplate;
        private readonly Mock<IClaimsParser> _parser;
        private List<ClaimToThing> _configuration;
        private List<Claim> _claims;
        private Response _result;
        private Response<string> _claimValue;
        private List<PlaceholderNameAndValue> _placeholderValues;

        public ChangeDownstreamPathTemplateTests()
        {
            _parser = new Mock<IClaimsParser>();
            _changeDownstreamPath = new ChangeDownstreamPathTemplate(_parser.Object);
        }

        [Fact]
        public void should_change_downstream_path_request()
        {
            var claims = new List<Claim>
            {
                new Claim("test", "data"),
            };
            var placeHolderValues = new List<PlaceholderNameAndValue>();
            this.Given(
                x => x.GivenAClaimToThing(new List<ClaimToThing>
                {
                    new ClaimToThing("path-key", "", "", 0),
                }))
                .And(x => x.GivenClaims(claims))
                .And(x => x.GivenDownstreamPathTemplate("/api/test/{path-key}"))
                .And(x => x.GivenPlaceholderNameAndValues(placeHolderValues))
                .And(x => x.GivenTheClaimParserReturns(new OkResponse<string>("value")))
                .When(x => x.WhenIChangeDownstreamPath())
                .Then(x => x.ThenTheResultIsSuccess())
                .And(x => x.ThenClaimDataIsContainedInPlaceHolder("{path-key}", "value"))
                .BDDfy();
        }

        [Fact]
        public void should_replace_existing_placeholder_value()
        {
            var claims = new List<Claim>
            {
                new Claim("test", "data"),
            };
            var placeHolderValues = new List<PlaceholderNameAndValue>
            {
                new PlaceholderNameAndValue ("{path-key}", "old_value"),
            };
            this.Given(
                x => x.GivenAClaimToThing(new List<ClaimToThing>
                {
                    new ClaimToThing("path-key", "", "", 0),
                }))
                .And(x => x.GivenClaims(claims))
                .And(x => x.GivenDownstreamPathTemplate("/api/test/{path-key}"))
                .And(x => x.GivenPlaceholderNameAndValues(placeHolderValues))
                .And(x => x.GivenTheClaimParserReturns(new OkResponse<string>("value")))
                .When(x => x.WhenIChangeDownstreamPath())
                .Then(x => x.ThenTheResultIsSuccess())
                .And(x => x.ThenClaimDataIsContainedInPlaceHolder("{path-key}", "value"))
                .BDDfy();
        }

        [Fact]
        public void should_return_error_when_no_placeholder_in_downstream_path()
        {
            var claims = new List<Claim>
            {
                new Claim("test", "data"),
            };
            var placeHolderValues = new List<PlaceholderNameAndValue>();
            this.Given(
                x => x.GivenAClaimToThing(new List<ClaimToThing>
                {
                    new ClaimToThing("path-key", "", "", 0),
                }))
                .And(x => x.GivenClaims(claims))
                .And(x => x.GivenDownstreamPathTemplate("/api/test"))
                .And(x => x.GivenPlaceholderNameAndValues(placeHolderValues))
                .And(x => x.GivenTheClaimParserReturns(new OkResponse<string>("value")))
                .When(x => x.WhenIChangeDownstreamPath())
                .Then(x => x.ThenTheResultIsCouldNotFindPlaceholderError())
                .BDDfy();
        }

        [Fact]
        private void should_return_error_when_claim_parser_returns_error()
        {
            var claims = new List<Claim>
            {
                new Claim("test", "data"),
            };
            var placeHolderValues = new List<PlaceholderNameAndValue>();
            this.Given(
                x => x.GivenAClaimToThing(new List<ClaimToThing>
                {
                    new ClaimToThing("path-key", "", "", 0),
                }))
                .And(x => x.GivenClaims(claims))
                .And(x => x.GivenDownstreamPathTemplate("/api/test/{path-key}"))
                .And(x => x.GivenPlaceholderNameAndValues(placeHolderValues))
                .And(x => x.GivenTheClaimParserReturns(new ErrorResponse<string>(new List<Error>
                {
                   new AnyError(),
                })))
                .When(x => x.WhenIChangeDownstreamPath())
                .Then(x => x.ThenTheResultIsError())
                .BDDfy();
        }

        private void GivenAClaimToThing(List<ClaimToThing> configuration)
        {
            _configuration = configuration;
        }

        private void GivenClaims(List<Claim> claims)
        {
            _claims = claims;
        }

        private void GivenDownstreamPathTemplate(string template)
        {
            _downstreamPathTemplate = new DownstreamPathTemplate(template);
        }

        private void GivenPlaceholderNameAndValues(List<PlaceholderNameAndValue> placeholders)
        {
            _placeholderValues = placeholders;
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

        private void WhenIChangeDownstreamPath()
        {
            _result = _changeDownstreamPath.ChangeDownstreamPath(_configuration, _claims,
                        _downstreamPathTemplate, _placeholderValues);
        }

        private void ThenTheResultIsSuccess()
        {
            _result.IsError.ShouldBe(false);
        }

        private void ThenTheResultIsCouldNotFindPlaceholderError()
        {
            _result.IsError.ShouldBe(true);
            _result.Errors.Count.ShouldBe(1);
            _result.Errors.First().ShouldBeOfType<CouldNotFindPlaceholderError>();
        }

        private void ThenTheResultIsError()
        {
            _result.IsError.ShouldBe(true);
        }

        private void ThenClaimDataIsContainedInPlaceHolder(string name, string value)
        {
            var placeHolder = _placeholderValues.FirstOrDefault(ph => ph.Name == name && ph.Value == value);
            placeHolder.ShouldNotBeNull();
            _placeholderValues.Count.ShouldBe(1);
        }
    }
}
