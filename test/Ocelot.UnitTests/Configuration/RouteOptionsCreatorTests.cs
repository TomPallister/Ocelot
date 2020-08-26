namespace Ocelot.UnitTests.Configuration
{
    using Ocelot.Configuration;
    using Ocelot.Configuration.Builder;
    using Ocelot.Configuration.Creator;
    using Ocelot.Configuration.File;
    using Shouldly;
    using System.Collections.Generic;
    using TestStack.BDDfy;
    using Xunit;

    public class RouteOptionsCreatorTests
    {
        private readonly RouteOptionsCreator _creator;
        private FileRoute _route;
        private RouteOptions _result;

        public RouteOptionsCreatorTests()
        {
            _creator = new RouteOptionsCreator();
        }

        [Fact]
        public void should_create_re_route_options()
        {
            var route = new FileRoute
            {
                RateLimitOptions = new FileRateLimitRule
                {
                    EnableRateLimiting = true
                },
                AuthenticationOptions = new FileAuthenticationOptions()
                {
                    AuthenticationProviderKey = "Test"
                },
                RouteClaimsRequirement = new Dictionary<string, string>()
                {
                    {"",""}
                },
                FileCacheOptions = new FileCacheOptions
                {
                    TtlSeconds = 1
                },
                ServiceName = "west"
            };

            var expected = new RouteOptionsBuilder()
                .WithIsAuthenticated(true)
                .WithIsAuthorised(true)
                .WithIsCached(true)
                .WithRateLimiting(true)
                .WithUseServiceDiscovery(true)
                .Build();

            this.Given(x => x.GivenTheFollowing(route))
                .When(x => x.WhenICreate())
                .Then(x => x.ThenTheFollowingIsReturned(expected))
                .BDDfy();
        }

        private void GivenTheFollowing(FileRoute route)
        {
            _route = route;
        }

        private void WhenICreate()
        {
            _result = _creator.Create(_route);
        }

        private void ThenTheFollowingIsReturned(RouteOptions expected)
        {
            _result.IsAuthenticated.ShouldBe(expected.IsAuthenticated);
            _result.IsAuthorised.ShouldBe(expected.IsAuthorised);
            _result.IsCached.ShouldBe(expected.IsCached);
            _result.EnableRateLimiting.ShouldBe(expected.EnableRateLimiting);
            _result.UseServiceDiscovery.ShouldBe(expected.UseServiceDiscovery);
        }
    }
}
