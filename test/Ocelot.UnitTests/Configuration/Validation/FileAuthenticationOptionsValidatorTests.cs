using FluentValidation.Results;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Moq;
using Ocelot.Configuration.File;
using Ocelot.Configuration.Validator;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.Configuration.Validation
{
    public class FileAuthenticationOptionsValidatorTests
    {
        private FileAuthenticationOptionsValidator _validator;
        private readonly Mock<IAuthenticationSchemeProvider> _authProvider;
        private FileAuthenticationOptions _authenticationOptions;
        private ValidationResult _result;

        public FileAuthenticationOptionsValidatorTests()
        {
            _authProvider = new Mock<IAuthenticationSchemeProvider>();
            _validator = new FileAuthenticationOptionsValidator(_authProvider.Object);
        }

        [Fact]
        public void should_be_valid_if_specified_authentication_provider_is_registered()
        {
            const string key = "JwtLads";

            var authenticationOptions = new FileAuthenticationOptions
            {
                AuthenticationProviderKey = key,
            };

            this.Given(_ => GivenThe(authenticationOptions))
                .And(_ => GivenAnAuthProvider(key))
                .When(_ => WhenIValidate())
                .Then(_ => ThenTheResultIsValid())
                .BDDfy();
        }

        [Fact]
        public void should_not_be_valid_if_specified_authentication_provider_is_not_registered()
        {
            const string key = "JwtLads";

            var authenticationOptions = new FileAuthenticationOptions
            {
                AuthenticationProviderKey = key,
            };

            this.Given(_ => GivenThe(authenticationOptions))
                .When(_ => WhenIValidate())
                .Then(_ => ThenTheResultIsNotValid())
                .And(_ => ThenTheErrorIs(key))
                .BDDfy();
        }

        private void GivenAnAuthProvider(string key)
        {
            var schemes = new List<AuthenticationScheme>
            {
                new AuthenticationScheme(key, key, typeof(FakeAuthHandler)),
            };

            _authProvider
                .Setup(x => x.GetAllSchemesAsync())
                .ReturnsAsync(schemes);
        }

        private void ThenTheErrorIs(string providerKey)
        {
            _result.Errors[0].ErrorMessage.ShouldBe($"Authentication Provider Key: {providerKey} is unsupported authentication provider");
        }

        private void ThenTheResultIsValid()
        {
            _result.IsValid.ShouldBeTrue();
        }

        private void ThenTheResultIsNotValid()
        {
            _result.IsValid.ShouldBeFalse();
        }        

        private void GivenThe(FileAuthenticationOptions authenticationOptions)
        {
            _authenticationOptions = authenticationOptions;
        }

        private void WhenIValidate()
        {
            _result = _validator.Validate(_authenticationOptions);
        }

        private class FakeAuthHandler : IAuthenticationHandler
        {
            public Task<AuthenticateResult> AuthenticateAsync()
            {
                throw new NotImplementedException();
            }

            public Task ChallengeAsync(AuthenticationProperties properties)
            {
                throw new NotImplementedException();
            }

            public Task ForbidAsync(AuthenticationProperties properties)
            {
                throw new NotImplementedException();
            }

            public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
            {
                throw new NotImplementedException();
            }
        }
    }
}
