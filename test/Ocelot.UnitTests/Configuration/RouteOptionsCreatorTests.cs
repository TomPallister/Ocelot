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
        private FileGlobalConfiguration _globalConfiguration;
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

            this.Given(x => x.GivenTheFollowing(route, null))
                .When(x => x.WhenICreate())
                .Then(x => x.ThenTheFollowingIsReturned(expected))
                .BDDfy();
        }

        [Fact]
        public void should_set_isauthenticated_to_true_when_providerkey_set_in_route()
        {
            var route = new FileRoute
            {
                RateLimitOptions = new FileRateLimitRule
                {
                    EnableRateLimiting = true,
                },
                RouteClaimsRequirement = new Dictionary<string, string>()
                {
                    {"",""},
                },
                FileCacheOptions = new FileCacheOptions
                {
                    TtlSeconds = 1,
                },
                ServiceName = "west",
                AuthenticationOptions = new FileAuthenticationOptions()
                {
                    AuthenticationProviderKey = "Test",
                },
            };
            var globalConf = new FileGlobalConfiguration
            {                
            };

            var expected = new RouteOptionsBuilder()
                .WithIsAuthenticated(true)
                .WithIsAuthorised(true)
                .WithIsCached(true)
                .WithRateLimiting(true)
                .WithUseServiceDiscovery(true)
                .Build();

            this.Given(x => x.GivenTheFollowing(route, globalConf))
                .When(x => x.WhenICreate())
                .Then(x => x.ThenTheFollowingIsReturned(expected))
                .BDDfy();
        }

        [Fact]
        public void should_set_isauthenticated_to_true_when_providerkey_set_in_globalconfiguration()
        {
            var route = new FileRoute
            {
                RateLimitOptions = new FileRateLimitRule
                {
                    EnableRateLimiting = true,
                },                
                RouteClaimsRequirement = new Dictionary<string, string>()
                {
                    {"",""},
                },
                FileCacheOptions = new FileCacheOptions
                {
                    TtlSeconds = 1,
                },
                ServiceName = "west",
            };
            var globalConf = new FileGlobalConfiguration
            {
                AuthenticationOptions = new FileAuthenticationOptions()
                {
                    AuthenticationProviderKey = "Test",
                },
            };

            var expected = new RouteOptionsBuilder()
                .WithIsAuthenticated(true)
                .WithIsAuthorised(true)
                .WithIsCached(true)
                .WithRateLimiting(true)
                .WithUseServiceDiscovery(true)
                .Build();

            this.Given(x => x.GivenTheFollowing(route, globalConf))
                .When(x => x.WhenICreate())
                .Then(x => x.ThenTheFollowingIsReturned(expected))
                .BDDfy();
        }

        [Fact]
        public void should_set_isauthenticated_to_false_when_providerkey_set_in_globalconfiguration_but_route_has_allowanonymous()
        {
            var route = new FileRoute
            {
                RateLimitOptions = new FileRateLimitRule
                {
                    EnableRateLimiting = true,
                },
                RouteClaimsRequirement = new Dictionary<string, string>()
                {
                    { "", "" },
                },
                FileCacheOptions = new FileCacheOptions
                {
                    TtlSeconds = 1,
                },
                ServiceName = "west",
                AuthenticationOptions = new FileAuthenticationOptions
                {
                    AllowAnonymous = true,
                },
            };
            var globalConf = new FileGlobalConfiguration
            {
                AuthenticationOptions = new FileAuthenticationOptions()
                {
                    AuthenticationProviderKey = "Test",
                },
            };

            var expected = new RouteOptionsBuilder()
                .WithIsAuthenticated(false)
                .WithIsAuthorised(true)
                .WithIsCached(true)
                .WithRateLimiting(true)
                .WithUseServiceDiscovery(true)
                .Build();

            this.Given(x => x.GivenTheFollowing(route, globalConf))
                .When(x => x.WhenICreate())
                .Then(x => x.ThenTheFollowingIsReturned(expected))
                .BDDfy();
        }

        [Fact]
        public void should_set_isauthenticated_to_false_when_providerkey_not_set_at_all()
        {
            var route = new FileRoute
            {
                RateLimitOptions = new FileRateLimitRule
                {
                    EnableRateLimiting = true,
                },
                RouteClaimsRequirement = new Dictionary<string, string>()
                {
                    {"",""},
                },
                FileCacheOptions = new FileCacheOptions
                {
                    TtlSeconds = 1,
                },
                ServiceName = "west",
            };
            var globalConf = new FileGlobalConfiguration
            {
            };

            var expected = new RouteOptionsBuilder()
                .WithIsAuthenticated(false)
                .WithIsAuthorised(true)
                .WithIsCached(true)
                .WithRateLimiting(true)
                .WithUseServiceDiscovery(true)
                .Build();

            this.Given(x => x.GivenTheFollowing(route, globalConf))
                .When(x => x.WhenICreate())
                .Then(x => x.ThenTheFollowingIsReturned(expected))
                .BDDfy();
        }

        private void GivenTheFollowing(FileRoute route, FileGlobalConfiguration globalConfiguration)
        {
            _route = route;
            _globalConfiguration = globalConfiguration;
        }

        private void WhenICreate()
        {
            _result = _creator.Create(_route, _globalConfiguration);
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
