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

    public class AuthenticationOptionsCreatorTests
    {
        private readonly AuthenticationOptionsCreator _authOptionsCreator;
        private FileRoute _fileRoute;
        private FileGlobalConfiguration _fileGlobalConfig;
        private AuthenticationOptions _result;

        public AuthenticationOptionsCreatorTests()
        {
            _authOptionsCreator = new AuthenticationOptionsCreator();
        }

        [Fact]
        public void should_return_auth_options()
        {
            var fileRoute = new FileRoute()
            {
                AuthenticationOptions = new FileAuthenticationOptions
                {
                    AuthenticationProviderKey = "Test",
                    AllowedScopes = new List<string> { "cheese" },
                }
            };
            var globalConfig = new FileGlobalConfiguration();

            var expected = new AuthenticationOptionsBuilder()
                    .WithAllowedScopes(fileRoute.AuthenticationOptions?.AllowedScopes)
                    .WithAuthenticationProviderKey("Test")
                    .Build();

            this.Given(x => x.GivenTheFollowingRoute(fileRoute))
                .And(x => x.GivenTheFollowingGlobalConfig(globalConfig))
                .When(x => x.WhenICreateTheAuthenticationOptions())
                .Then(x => x.ThenTheFollowingConfigIsReturned(expected))
                .BDDfy();
        }

        [Fact]
        public void should_use_global_configuration()
        {
            var route = new FileRoute();
            var globalConfig = new FileGlobalConfiguration
            {
                AuthenticationOptions = new FileAuthenticationOptions()
                {
                    AuthenticationProviderKey = "key",
                    AllowedScopes = new List<string>() { "scope1", "scope2" },
                },
            };

            var expected = new AuthenticationOptionsBuilder()
                .WithAllowedScopes(globalConfig.AuthenticationOptions?.AllowedScopes)
                .WithAuthenticationProviderKey(globalConfig.AuthenticationOptions?.AuthenticationProviderKey)
                .Build();

            this.Given(x => x.GivenTheFollowingRoute(route))
                .And(x => x.GivenTheFollowingGlobalConfig(globalConfig))
                .When(x => x.WhenICreateTheAuthenticationOptions())
                .Then(x => x.ThenTheFollowingConfigIsReturned(expected))
                .BDDfy();
        }

        [Fact]
        public void should_use_global_configuration_when_route_provider_key_is_empty()
        {
            var route = new FileRoute
            {
                AuthenticationOptions = new FileAuthenticationOptions
                {
                    AuthenticationProviderKey = "",
                    AllowedScopes = new List<string> { "cheese" },
                },
            };
            var globalConfig = new FileGlobalConfiguration
            {
                AuthenticationOptions = new FileAuthenticationOptions()
                {
                    AuthenticationProviderKey = "key",
                    AllowedScopes = new List<string>() { "scope1", "scope2" },
                },
            };

            var expected = new AuthenticationOptionsBuilder()
                .WithAllowedScopes(globalConfig.AuthenticationOptions?.AllowedScopes)
                .WithAuthenticationProviderKey(globalConfig.AuthenticationOptions?.AuthenticationProviderKey)
                .Build();

            this.Given(x => x.GivenTheFollowingRoute(route))
                .And(x => x.GivenTheFollowingGlobalConfig(globalConfig))
                .When(x => x.WhenICreateTheAuthenticationOptions())
                .Then(x => x.ThenTheFollowingConfigIsReturned(expected))
                .BDDfy();
        }

        [Fact]
        public void should_use_route_over_global_specific()
        {
            var route = new FileRoute
            {
                AuthenticationOptions = new FileAuthenticationOptions()
                {
                    AuthenticationProviderKey = "routeKey",
                    AllowedScopes = new List<string>() { "routeScope1", "routeScope2" },
                },
            };
            var globalConfig = new FileGlobalConfiguration
            {
                AuthenticationOptions = new FileAuthenticationOptions()
                {
                    AuthenticationProviderKey = "globalKey",
                    AllowedScopes = new List<string>() { "globalScope1", "globalScope2" },
                },
            };

            var expected = new AuthenticationOptionsBuilder()
                   .WithAllowedScopes(route.AuthenticationOptions?.AllowedScopes)
                   .WithAuthenticationProviderKey(route.AuthenticationOptions?.AuthenticationProviderKey)
                   .Build();

            this.Given(x => x.GivenTheFollowingRoute(route))
                .And(x => x.GivenTheFollowingGlobalConfig(globalConfig))
                .When(x => x.WhenICreateTheAuthenticationOptions())
                .Then(x => x.ThenTheFollowingConfigIsReturned(expected))
                .BDDfy();
        }

        private void GivenTheFollowingRoute(FileRoute fileRoute)
        {
            _fileRoute = fileRoute;
        }

        private void GivenTheFollowingGlobalConfig(FileGlobalConfiguration globalConfig)
        {
            _fileGlobalConfig = globalConfig;
        }

        private void WhenICreateTheAuthenticationOptions()
        {
            _result = _authOptionsCreator.Create(_fileRoute.AuthenticationOptions, _fileGlobalConfig.AuthenticationOptions);
        }

        private void ThenTheFollowingConfigIsReturned(AuthenticationOptions expected)
        {
            _result.AllowedScopes.ShouldBe(expected.AllowedScopes);
            _result.AuthenticationProviderKey.ShouldBe(expected.AuthenticationProviderKey);
        }
    }
}
