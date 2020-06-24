using Microsoft.AspNetCore.Http;
using Moq;
using Ocelot.Claims;
using Ocelot.Configuration;
using Ocelot.Errors;
using Ocelot.Infrastructure.Claims.Parser;
using Ocelot.Responses;
using Shouldly;
using System.Collections.Generic;
using System.Security.Claims;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.Claims
{
    public class AddClaimsToRequestTests
    {
        private readonly AddClaimsToRequest _addClaimsToRequest;
        private readonly Mock<IClaimsParser> _parser;
        private List<ClaimToThing> _claimsToThings;
        private HttpContext _context;
        private Response _result;
        private Response<string> _claimValue;

        public AddClaimsToRequestTests()
        {
            _parser = new Mock<IClaimsParser>();
            _addClaimsToRequest = new AddClaimsToRequest(_parser.Object);
        }

        [Fact]
        public void should_add_claims_to_context()
        {
            var context = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                    new Claim("test", "data")
                }))
            };

            this.Given(
                x => x.GivenClaimsToThings(new List<ClaimToThing>
                {
                    new ClaimToThing("claim-key", "", "", 0)
                }))
                .Given(x => x.GivenHttpContext(context))
                .And(x => x.GivenTheClaimParserReturns(new OkResponse<string>("value")))
                .When(x => x.WhenIAddClaimsToTheRequest())
                .Then(x => x.ThenTheResultIsSuccess())
                .BDDfy();
        }

        [Fact]
        public void if_claims_exists_should_replace_it()
        {
            var context = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                    new Claim("existing-key", "data"),
                    new Claim("new-key", "data")
                })),
            };

            this.Given(
                x => x.GivenClaimsToThings(new List<ClaimToThing>
                {
                    new ClaimToThing("existing-key", "new-key", "", 0)
                }))
                .Given(x => x.GivenHttpContext(context))
                .And(x => x.GivenTheClaimParserReturns(new OkResponse<string>("value")))
                .When(x => x.WhenIAddClaimsToTheRequest())
                .Then(x => x.ThenTheResultIsSuccess())
                .BDDfy();
        }

        [Fact]
        public void should_return_error()
        {
            this.Given(
               x => x.GivenClaimsToThings(new List<ClaimToThing>
               {
                    new ClaimToThing("", "", "", 0)
               }))
               .Given(x => x.GivenHttpContext(new DefaultHttpContext()))
               .And(x => x.GivenTheClaimParserReturns(new ErrorResponse<string>(new List<Error>
               {
                   new AnyError()
               })))
               .When(x => x.WhenIAddClaimsToTheRequest())
               .Then(x => x.ThenTheResultIsError())
               .BDDfy();
        }

        private void GivenClaimsToThings(List<ClaimToThing> configuration)
        {
            _claimsToThings = configuration;
        }

        private void GivenHttpContext(HttpContext context)
        {
            _context = context;
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

        private void WhenIAddClaimsToTheRequest()
        {
            _result = _addClaimsToRequest.SetClaimsOnContext(_claimsToThings, _context);
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
