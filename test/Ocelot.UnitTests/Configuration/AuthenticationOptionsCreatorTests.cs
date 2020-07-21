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

            var expected = new AuthenticationOptionsBuilder()
                    .WithAllowedScopes(fileRoute.AuthenticationOptions?.AllowedScopes)
                    .WithAuthenticationProviderKey("Test")
                    .Build();

            this.Given(x => x.GivenTheFollowing(fileRoute))
                .When(x => x.WhenICreateTheAuthenticationOptions())
                .Then(x => x.ThenTheFollowingConfigIsReturned(expected))
                .BDDfy();
        }

        private void GivenTheFollowing(FileRoute fileRoute)
        {
            _fileRoute = fileRoute;
        }

        private void WhenICreateTheAuthenticationOptions()
        {
            _result = _authOptionsCreator.Create(_fileRoute);
        }

        private void ThenTheFollowingConfigIsReturned(AuthenticationOptions expected)
        {
            _result.AllowedScopes.ShouldBe(expected.AllowedScopes);
            _result.AuthenticationProviderKey.ShouldBe(expected.AuthenticationProviderKey);
        }
    }
}
