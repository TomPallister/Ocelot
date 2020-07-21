using Ocelot.Authorisation;
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

    public class ClaimsAuthoriserTests
    {
        private readonly ClaimsAuthoriser _claimsAuthoriser;
        private ClaimsPrincipal _claimsPrincipal;
        private Dictionary<string, string> _requirement;
        private List<PlaceholderNameAndValue> _urlPathPlaceholderNameAndValues;
        private Response<bool> _result;

        public ClaimsAuthoriserTests()
        {
            _claimsAuthoriser = new ClaimsAuthoriser(new ClaimsParser());
        }

        [Fact]
        public void should_authorise_user()
        {
            this.Given(x => x.GivenAClaimsPrincipal(new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                    new Claim("UserType", "registered"),
                }))))
                .And(x => x.GivenARouteClaimsRequirement(new Dictionary<string, string>
                {
                    {"UserType", "registered"}
                }))
                .When(x => x.WhenICallTheAuthoriser())
                .Then(x => x.ThenTheUserIsAuthorised())
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
               .When(x => x.WhenICallTheAuthoriser())
               .Then(x => x.ThenTheUserIsAuthorised())
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
               .When(x => x.WhenICallTheAuthoriser())
               .Then(x => x.ThenTheUserIsntAuthorised())
               .BDDfy();
        }

        [Fact]
        public void should_authorise_user_multiple_claims_of_same_type()
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
                .When(x => x.WhenICallTheAuthoriser())
                .Then(x => x.ThenTheUserIsAuthorised())
                .BDDfy();
        }

        [Fact]
        public void should_not_authorise_user()
        {
            this.Given(x => x.GivenAClaimsPrincipal(new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>()))))
            .And(x => x.GivenARouteClaimsRequirement(new Dictionary<string, string>
                {
                    { "UserType", "registered" }
                }))
            .When(x => x.WhenICallTheAuthoriser())
            .Then(x => x.ThenTheUserIsntAuthorised())
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

        private void WhenICallTheAuthoriser()
        {
            _result = _claimsAuthoriser.Authorise(_claimsPrincipal, _requirement, _urlPathPlaceholderNameAndValues);
        }

        private void ThenTheUserIsAuthorised()
        {
            _result.Data.ShouldBe(true);
        }

        private void ThenTheUserIsntAuthorised()
        {
            _result.Data.ShouldBe(false);
        }
    }
}
