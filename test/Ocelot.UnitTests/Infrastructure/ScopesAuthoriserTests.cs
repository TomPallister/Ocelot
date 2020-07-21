using Moq;
using Ocelot.Authorisation;
using Ocelot.Errors;
using Ocelot.Infrastructure.Claims.Parser;
using Ocelot.Responses;
using Shouldly;
using System.Collections.Generic;
using System.Security.Claims;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.Infrastructure
{
    public class ScopesAuthoriserTests
    {
        private ScopesAuthoriser _authoriser;
        public Mock<IClaimsParser> _parser;
        private ClaimsPrincipal _principal;
        private List<string> _allowedScopes;
        private Response<bool> _result;

        public ScopesAuthoriserTests()
        {
            _parser = new Mock<IClaimsParser>();
            _authoriser = new ScopesAuthoriser(_parser.Object);
        }

        [Fact]
        public void should_return_ok_if_no_allowed_scopes()
        {
            this.Given(_ => GivenTheFollowing(new ClaimsPrincipal()))
            .And(_ => GivenTheFollowing(new List<string>()))
            .When(_ => WhenIAuthorise())
            .Then(_ => ThenTheFollowingIsReturned(new OkResponse<bool>(true)))
            .BDDfy();
        }

        [Fact]
        public void should_return_ok_if_null_allowed_scopes()
        {
            this.Given(_ => GivenTheFollowing(new ClaimsPrincipal()))
            .And(_ => GivenTheFollowing((List<string>)null))
            .When(_ => WhenIAuthorise())
            .Then(_ => ThenTheFollowingIsReturned(new OkResponse<bool>(true)))
            .BDDfy();
        }

        [Fact]
        public void should_return_error_if_claims_parser_returns_error()
        {
            var fakeError = new FakeError();
            this.Given(_ => GivenTheFollowing(new ClaimsPrincipal()))
            .And(_ => GivenTheParserReturns(new ErrorResponse<List<string>>(fakeError)))
            .And(_ => GivenTheFollowing(new List<string>() { "doesntmatter" }))
            .When(_ => WhenIAuthorise())
            .Then(_ => ThenTheFollowingIsReturned(new ErrorResponse<bool>(fakeError)))
            .BDDfy();
        }

        [Fact]
        public void should_match_scopes_and_return_ok_result()
        {
            var claimsPrincipal = new ClaimsPrincipal();
            var allowedScopes = new List<string>() { "someScope" };

            this.Given(_ => GivenTheFollowing(claimsPrincipal))
            .And(_ => GivenTheParserReturns(new OkResponse<List<string>>(allowedScopes)))
            .And(_ => GivenTheFollowing(allowedScopes))
            .When(_ => WhenIAuthorise())
            .Then(_ => ThenTheFollowingIsReturned(new OkResponse<bool>(true)))
            .BDDfy();
        }

        [Fact]
        public void should_not_match_scopes_and_return_error_result()
        {
            var fakeError = new FakeError();
            var claimsPrincipal = new ClaimsPrincipal();
            var allowedScopes = new List<string>() { "someScope" };
            var userScopes = new List<string>() { "anotherScope" };

            this.Given(_ => GivenTheFollowing(claimsPrincipal))
            .And(_ => GivenTheParserReturns(new OkResponse<List<string>>(userScopes)))
            .And(_ => GivenTheFollowing(allowedScopes))
            .When(_ => WhenIAuthorise())
            .Then(_ => ThenTheFollowingIsReturned(new ErrorResponse<bool>(fakeError)))
            .BDDfy();
        }

        private void GivenTheParserReturns(Response<List<string>> response)
        {
            _parser.Setup(x => x.GetValuesByClaimType(It.IsAny<IEnumerable<Claim>>(), It.IsAny<string>())).Returns(response);
        }

        private void GivenTheFollowing(ClaimsPrincipal principal)
        {
            _principal = principal;
        }

        private void GivenTheFollowing(List<string> allowedScopes)
        {
            _allowedScopes = allowedScopes;
        }

        private void WhenIAuthorise()
        {
            _result = _authoriser.Authorise(_principal, _allowedScopes);
        }

        private void ThenTheFollowingIsReturned(Response<bool> expected)
        {
            _result.Data.ShouldBe(expected.Data);
            _result.IsError.ShouldBe(expected.IsError);
        }
    }

    public class FakeError : Error
    {
        public FakeError() : base("fake error", OcelotErrorCode.CannotAddDataError, 404)
        {
        }
    }
}
