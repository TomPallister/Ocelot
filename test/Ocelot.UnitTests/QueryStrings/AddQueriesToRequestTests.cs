using Moq;
using Ocelot.Configuration;
using Ocelot.Errors;
using Ocelot.Infrastructure.Claims.Parser;
using Ocelot.QueryStrings;
using Ocelot.Request.Middleware;
using Ocelot.Responses;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.QueryStrings
{
    public class AddQueriesToRequestTests
    {
        private readonly AddQueriesToRequest _addQueriesToRequest;
        private DownstreamRequest _downstreamRequest;
        private readonly Mock<IClaimsParser> _parser;
        private List<ClaimToThing> _configuration;
        private List<Claim> _claims;
        private Response _result;
        private Response<string> _claimValue;
        private HttpRequestMessage _request;

        public AddQueriesToRequestTests()
        {
            _request = new HttpRequestMessage(HttpMethod.Post, "http://my.url/abc?q=123");
            _parser = new Mock<IClaimsParser>();
            _addQueriesToRequest = new AddQueriesToRequest(_parser.Object);
            _downstreamRequest = new DownstreamRequest(_request);
        }

        [Fact]
        public void should_add_new_queries_to_downstream_request()
        {
            var claims = new List<Claim>
            {
                new Claim("test", "data")
            };

            this.Given(
                x => x.GivenAClaimToThing(new List<ClaimToThing>
                {
                    new ClaimToThing("query-key", "", "", 0)
                }))
                .Given(x => x.GivenClaims(claims))
                .And(x => x.GivenTheClaimParserReturns(new OkResponse<string>("value")))
                .When(x => x.WhenIAddQueriesToTheRequest())
                .Then(x => x.ThenTheResultIsSuccess())
                .And(x => x.ThenTheQueryIsAdded())
                .BDDfy();
        }

        [Fact]
        public void should_add_new_queries_to_downstream_request_and_preserve_other_queries()
        {
            var claims = new List<Claim>
            {
                new Claim("test", "data")
            };

            this.Given(
                x => x.GivenAClaimToThing(new List<ClaimToThing>
                {
                    new ClaimToThing("query-key", "", "", 0)
                }))
                .Given(x => x.GivenClaims(claims))
                .And(x => GivenTheDownstreamRequestHasQueryString("?test=1&test=2"))
                .And(x => x.GivenTheClaimParserReturns(new OkResponse<string>("value")))
                .When(x => x.WhenIAddQueriesToTheRequest())
                .Then(x => x.ThenTheResultIsSuccess())
                .And(x => x.ThenTheQueryIsAdded())
                .And(x => TheTheQueryStringIs("?test=1&test=2&query-key=value"))
                .BDDfy();
        }

        private void TheTheQueryStringIs(string expected)
        {
            _downstreamRequest.Query.ShouldBe(expected);
        }

        [Fact]
        public void should_replace_existing_queries_on_downstream_request()
        {
            var claims = new List<Claim>
            {
                new Claim("test", "data")
            };

            this.Given(
                x => x.GivenAClaimToThing(new List<ClaimToThing>
                {
                    new ClaimToThing("query-key", "", "", 0)
                }))
                .And(x => x.GivenClaims(claims))
                .And(x => x.GivenTheDownstreamRequestHasQueryString("query-key", "initial"))
                .And(x => x.GivenTheClaimParserReturns(new OkResponse<string>("value")))
                .When(x => x.WhenIAddQueriesToTheRequest())
                .Then(x => x.ThenTheResultIsSuccess())
                .And(x => x.ThenTheQueryIsAdded())
                .BDDfy();
        }

        [Fact]
        public void should_return_error()
        {
            this.Given(
               x => x.GivenAClaimToThing(new List<ClaimToThing>
               {
                    new ClaimToThing("", "", "", 0)
               }))
               .Given(x => x.GivenClaims(new List<Claim>()))
               .And(x => x.GivenTheClaimParserReturns(new ErrorResponse<string>(new List<Error>
               {
                   new AnyError()
               })))
               .When(x => x.WhenIAddQueriesToTheRequest())
               .Then(x => x.ThenTheResultIsError())
               .BDDfy();
        }

        private void ThenTheQueryIsAdded()
        {
            var queries = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(_downstreamRequest.ToHttpRequestMessage().RequestUri.OriginalString);
            var query = queries.First(x => x.Key == "query-key");
            query.Value.First().ShouldBe(_claimValue.Data);
        }

        private void GivenAClaimToThing(List<ClaimToThing> configuration)
        {
            _configuration = configuration;
        }

        private void GivenClaims(List<Claim> claims)
        {
            _claims = claims;
        }

        private void GivenTheDownstreamRequestHasQueryString(string queryString)
        {
            _request = new HttpRequestMessage(HttpMethod.Post, $"http://my.url/abc{queryString}");
            _downstreamRequest = new DownstreamRequest(_request);
        }

        private void GivenTheDownstreamRequestHasQueryString(string key, string value)
        {
            var newUri = Microsoft.AspNetCore.WebUtilities.QueryHelpers
                .AddQueryString(_downstreamRequest.ToHttpRequestMessage().RequestUri.OriginalString, key, value);

            _request.RequestUri = new Uri(newUri);
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

        private void WhenIAddQueriesToTheRequest()
        {
            _result = _addQueriesToRequest.SetQueriesOnDownstreamRequest(_configuration, _claims, _downstreamRequest);
        }

        private void ThenTheResultIsSuccess()
        {
            _result.IsError.ShouldBe(false);
        }

        private void ThenTheResultIsError()
        {
            _result.IsError.ShouldBe(true);
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
