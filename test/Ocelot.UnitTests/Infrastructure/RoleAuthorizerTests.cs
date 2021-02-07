using Moq;
using Ocelot.Authorization;
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
    public class RoleAuthorizerTests
    {
        private RolesAuthorizer _authorizer;
        public Mock<IClaimsParser> _parser;
        private ClaimsPrincipal _principal;
        private List<string> _requiredRole;
        private Response<bool> _result;

        public RoleAuthorizerTests()
        {
            _parser = new Mock<IClaimsParser>();
            _authorizer = new RolesAuthorizer(_parser.Object);
        }

        [Fact]
        public void should_return_ok_if_no_allowed_scopes()
        {
            this.Given(_ => GivenTheFollowing(new ClaimsPrincipal()))
            .And(_ => GivenTheFollowing(new List<string>()))
            .When(_ => WhenIAuthorize())
            .Then(_ => ThenTheFollowingIsReturned(new OkResponse<bool>(true)))
            .BDDfy();
        }

        [Fact]
        public void should_return_ok_if_null_allowed_scopes()
        {
            this.Given(_ => GivenTheFollowing(new ClaimsPrincipal()))
            .And(_ => GivenTheFollowing((List<string>)null))
            .When(_ => WhenIAuthorize())
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
            .When(_ => WhenIAuthorize())
            .Then(_ => ThenTheFollowingIsReturned(new ErrorResponse<bool>(fakeError)))
            .BDDfy();
        }

        [Fact]
        public void should_match_role_and_return_ok_result()
        {
            var claimsPrincipal = new ClaimsPrincipal();
            var requiredRole = new List<string>() { "someRole" };

            this.Given(_ => GivenTheFollowing(claimsPrincipal))
            .And(_ => GivenTheParserReturns(new OkResponse<List<string>>(requiredRole)))
            .And(_ => GivenTheFollowing(requiredRole))
            .When(_ => WhenIAuthorize())
            .Then(_ => ThenTheFollowingIsReturned(new OkResponse<bool>(true)))
            .BDDfy();
        }

        [Fact]
        public void should_not_match_role_and_return_error_result()
        {
            var fakeError = new FakeError();
            var claimsPrincipal = new ClaimsPrincipal();
            var requiredRole = new List<string>() { "someRole" };
            var userRoles = new List<string>() { "anotherRole" };

            this.Given(_ => GivenTheFollowing(claimsPrincipal))
            .And(_ => GivenTheParserReturns(new OkResponse<List<string>>(userRoles)))
            .And(_ => GivenTheFollowing(requiredRole))
            .When(_ => WhenIAuthorize())
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

        private void GivenTheFollowing(List<string> requiredRole)
        {
            _requiredRole = requiredRole;
        }

        private void WhenIAuthorize()
        {
            _result = _authorizer.Authorize(_principal, _requiredRole, null);
        }

        private void ThenTheFollowingIsReturned(Response<bool> expected)
        {
            _result.Data.ShouldBe(expected.Data);
            _result.IsError.ShouldBe(expected.IsError);
        }
    }

}
