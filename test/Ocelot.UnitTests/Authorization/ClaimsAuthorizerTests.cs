using Ocelot.Authorization;
using Ocelot.DownstreamRouteFinder.UrlMatcher;
using Ocelot.Responses;
using Shouldly;
using System.Collections.Generic;
using System.Security.Claims;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.Authorization
{
    using Ocelot.Infrastructure.Claims.Parser;

    public class ClaimsAuthorizerTests
    {
        private readonly ClaimsAuthorizer _claimsAuthorizer;
        private ClaimsPrincipal _claimsPrincipal;
        private Dictionary<string, string> _requirement;
        private List<PlaceholderNameAndValue> _urlPathPlaceholderNameAndValues;
        private Response<bool> _result;

        public ClaimsAuthorizerTests()
        {
            _claimsAuthorizer = new ClaimsAuthorizer(new ClaimsParser());
        }

        [Fact]
        public void should_authorize_user()
        {
            this.Given(x => x.GivenAClaimsPrincipal(new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                    new Claim("UserType", "registered"),
                }))))
                .And(x => x.GivenARouteClaimsRequirement(new Dictionary<string, string>
                {
                    {"UserType", "registered"}
                }))
                .When(x => x.WhenICallTheAuthorizer())
                .Then(x => x.ThenTheUserIsAuthorized())
                .BDDfy();
        }

        [Fact]
        public void should_authorize_dynamic_user()
        {
            this.Given(x => x.GivenAClaimsPrincipal(new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                    new Claim("userid", "14"),
                }))))
               .And(x => x.GivenARouteClaimsRequirement(new Dictionary<string, string>
                {
                    {"userid", "{userId}"}
                }))
               .And(x => x.GivenAPlaceHolderNameAndValueList(new List<PlaceholderNameAndValue>
                {
                   new PlaceholderNameAndValue("{userId}", "14")
                }))
               .When(x => x.WhenICallTheAuthorizer())
               .Then(x => x.ThenTheUserIsAuthorized())
               .BDDfy();
        }

        [Fact]
        public void should_not_authorize_dynamic_user()
        {
            this.Given(x => x.GivenAClaimsPrincipal(new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                    new Claim("userid", "15"),
                }))))
               .And(x => x.GivenARouteClaimsRequirement(new Dictionary<string, string>
                {
                    {"userid", "{userId}"}
                }))
               .And(x => x.GivenAPlaceHolderNameAndValueList(new List<PlaceholderNameAndValue>
                {
                    new PlaceholderNameAndValue("{userId}", "14")
                }))
               .When(x => x.WhenICallTheAuthorizer())
               .Then(x => x.ThenTheUserIsntAuthorized())
               .BDDfy();
        }

        [Fact]
        public void should_authorize_user_multiple_claims_of_same_type()
        {
            this.Given(x => x.GivenAClaimsPrincipal(new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                    new Claim("UserType", "guest"),
                    new Claim("UserType", "registered"),
                }))))
                .And(x => x.GivenARouteClaimsRequirement(new Dictionary<string, string>
                {
                    {"UserType", "registered"}
                }))
                .When(x => x.WhenICallTheAuthorizer())
                .Then(x => x.ThenTheUserIsAuthorized())
                .BDDfy();
        }

        [Fact]
        public void should_not_authorize_user()
        {
            this.Given(x => x.GivenAClaimsPrincipal(new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>()))))
            .And(x => x.GivenARouteClaimsRequirement(new Dictionary<string, string>
                {
                    { "UserType", "registered" }
                }))
            .When(x => x.WhenICallTheAuthorizer())
            .Then(x => x.ThenTheUserIsntAuthorized())
            .BDDfy();
        }

        private void GivenAClaimsPrincipal(ClaimsPrincipal claimsPrincipal)
        {
            _claimsPrincipal = claimsPrincipal;
        }

        private void GivenARouteClaimsRequirement(Dictionary<string, string> requirement)
        {
            _requirement = requirement;
        }

        private void GivenAPlaceHolderNameAndValueList(List<PlaceholderNameAndValue> urlPathPlaceholderNameAndValues)
        {
            _urlPathPlaceholderNameAndValues = urlPathPlaceholderNameAndValues;
        }

        private void WhenICallTheAuthorizer()
        {
            _result = _claimsAuthorizer.Authorize(_claimsPrincipal, _requirement, _urlPathPlaceholderNameAndValues);
        }

        private void ThenTheUserIsAuthorized()
        {
            _result.Data.ShouldBe(true);
        }

        private void ThenTheUserIsntAuthorized()
        {
            _result.Data.ShouldBe(false);
        }
    }
}
