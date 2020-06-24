namespace Ocelot.UnitTests.Configuration.Validation
{
    using FluentValidation.Results;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Ocelot.Configuration.File;
    using Ocelot.Configuration.Validator;
    using Ocelot.Requester;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TestStack.BDDfy;
    using Xunit;

    public class RouteFluentValidatorTests
    {
        private readonly RouteFluentValidator _validator;
        private readonly Mock<IAuthenticationSchemeProvider> _authProvider;
        private QosDelegatingHandlerDelegate _qosDelegatingHandler;
        private Mock<IServiceProvider> _serviceProvider;
        private FileRoute _route;
        private ValidationResult _result;

        public RouteFluentValidatorTests()
        {
            _authProvider = new Mock<IAuthenticationSchemeProvider>();
            _serviceProvider = new Mock<IServiceProvider>();
            // Todo - replace with mocks
            _validator = new RouteFluentValidator(_authProvider.Object, new HostAndPortValidator(), new FileQoSOptionsFluentValidator(_serviceProvider.Object));
        }

        [Fact]
        public void downstream_path_template_should_not_be_empty()
        {
            var fileRoute = new FileRoute();

            this.Given(_ => GivenThe(fileRoute))
                .When(_ => WhenIValidate())
                .Then(_ => ThenTheResultIsInvalid())
                .And(_ => ThenTheErrorsContains("Downstream Path Template cannot be empty"))
                .BDDfy();
        }

        [Fact]
        public void upstream_path_template_should_not_be_empty()
        {
            var fileRoute = new FileRoute
            {
                DownstreamPathTemplate = "test"
            };

            this.Given(_ => GivenThe(fileRoute))
                .When(_ => WhenIValidate())
                .Then(_ => ThenTheResultIsInvalid())
                .And(_ => ThenTheErrorsContains("Upstream Path Template cannot be empty"))
                .BDDfy();
        }

        [Fact]
        public void downstream_path_template_should_start_with_forward_slash()
        {
            var fileRoute = new FileRoute
            {
                DownstreamPathTemplate = "test"
            };

            this.Given(_ => GivenThe(fileRoute))
                .When(_ => WhenIValidate())
                .Then(_ => ThenTheResultIsInvalid())
                .And(_ => ThenTheErrorsContains("Downstream Path Template test doesnt start with forward slash"))
                .BDDfy();
        }

        [Fact]
        public void downstream_path_template_should_not_contain_double_forward_slash()
        {
            var fileRoute = new FileRoute
            {
                DownstreamPathTemplate = "//test"
            };

            this.Given(_ => GivenThe(fileRoute))
                .When(_ => WhenIValidate())
                .Then(_ => ThenTheResultIsInvalid())
                .And(_ => ThenTheErrorsContains("Downstream Path Template //test contains double forward slash, Ocelot does not support this at the moment. Please raise an issue in GitHib if you need this feature."))
                .BDDfy();
        }

        [Theory]
        [InlineData("https://test")]
        [InlineData("http://test")]
        [InlineData("/test/http://")]
        [InlineData("/test/https://")]
        public void downstream_path_template_should_not_contain_scheme(string downstreamPathTemplate)
        {
            var fileRoute = new FileRoute
            {
                DownstreamPathTemplate = downstreamPathTemplate
            };

            this.Given(_ => GivenThe(fileRoute))
                .When(_ => WhenIValidate())
                .Then(_ => ThenTheResultIsInvalid())
                .And(_ => ThenTheErrorsContains($"Downstream Path Template {downstreamPathTemplate} contains double forward slash, Ocelot does not support this at the moment. Please raise an issue in GitHib if you need this feature."))
                .BDDfy();
        }

        [Fact]
        public void upstream_path_template_should_start_with_forward_slash()
        {
            var fileRoute = new FileRoute
            {
                DownstreamPathTemplate = "/test",
                UpstreamPathTemplate = "test"
            };

            this.Given(_ => GivenThe(fileRoute))
                .When(_ => WhenIValidate())
                .Then(_ => ThenTheResultIsInvalid())
                .And(_ => ThenTheErrorsContains("Upstream Path Template test doesnt start with forward slash"))
                .BDDfy();
        }

        [Fact]
        public void upstream_path_template_should_not_contain_double_forward_slash()
        {
            var fileRoute = new FileRoute
            {
                DownstreamPathTemplate = "/test",
                UpstreamPathTemplate = "//test"
            };

            this.Given(_ => GivenThe(fileRoute))
                .When(_ => WhenIValidate())
                .Then(_ => ThenTheResultIsInvalid())
                .And(_ => ThenTheErrorsContains("Upstream Path Template //test contains double forward slash, Ocelot does not support this at the moment. Please raise an issue in GitHib if you need this feature."))
                .BDDfy();
        }

        [Theory]
        [InlineData("https://test")]
        [InlineData("http://test")]
        [InlineData("/test/http://")]
        [InlineData("/test/https://")]
        public void upstream_path_template_should_not_contain_scheme(string upstreamPathTemplate)
        {
            var fileRoute = new FileRoute
            {
                DownstreamPathTemplate = "/test",
                UpstreamPathTemplate = upstreamPathTemplate
            };

            this.Given(_ => GivenThe(fileRoute))
                .When(_ => WhenIValidate())
                .Then(_ => ThenTheResultIsInvalid())
                .And(_ => ThenTheErrorsContains($"Upstream Path Template {upstreamPathTemplate} contains double forward slash, Ocelot does not support this at the moment. Please raise an issue in GitHib if you need this feature."))
                .BDDfy();
        }

        [Fact]
        public void should_not_be_valid_if_enable_rate_limiting_true_and_period_is_empty()
        {
            var fileRoute = new FileRoute
            {
                DownstreamPathTemplate = "/test",
                UpstreamPathTemplate = "/test",
                RateLimitOptions = new FileRateLimitRule
                {
                    EnableRateLimiting = true
                }
            };

            this.Given(_ => GivenThe(fileRoute))
                .When(_ => WhenIValidate())
                .Then(_ => ThenTheResultIsInvalid())
                .And(_ => ThenTheErrorsContains("RateLimitOptions.Period is empty"))
                .BDDfy();
        }

        [Fact]
        public void should_not_be_valid_if_enable_rate_limiting_true_and_period_has_value()
        {
            var fileRoute = new FileRoute
            {
                DownstreamPathTemplate = "/test",
                UpstreamPathTemplate = "/test",
                RateLimitOptions = new FileRateLimitRule
                {
                    EnableRateLimiting = true,
                    Period = "test"
                }
            };

            this.Given(_ => GivenThe(fileRoute))
                .When(_ => WhenIValidate())
                .Then(_ => ThenTheResultIsInvalid())
                .And(_ => ThenTheErrorsContains("RateLimitOptions.Period does not contain integer then s (second), m (minute), h (hour), d (day) e.g. 1m for 1 minute period"))
                .BDDfy();
        }

        [Fact]
        public void should_not_be_valid_if_specified_authentication_provider_isnt_registered()
        {
            var fileRoute = new FileRoute
            {
                DownstreamPathTemplate = "/test",
                UpstreamPathTemplate = "/test",
                AuthenticationOptions = new FileAuthenticationOptions
                {
                    AuthenticationProviderKey = "JwtLads"
                }
            };

            this.Given(_ => GivenThe(fileRoute))
                .When(_ => WhenIValidate())
                .Then(_ => ThenTheResultIsInvalid())
                .And(_ => ThenTheErrorsContains($"Authentication Options AuthenticationProviderKey:JwtLads,AllowedScopes:[] is unsupported authentication provider"))
                .BDDfy();
        }

        [Fact]
        public void should_not_be_valid_if_not_using_service_discovery_and_no_host_and_ports()
        {
            var fileRoute = new FileRoute
            {
                DownstreamPathTemplate = "/test",
                UpstreamPathTemplate = "/test",
            };

            this.Given(_ => GivenThe(fileRoute))
                .When(_ => WhenIValidate())
                .Then(_ => ThenTheResultIsInvalid())
                .And(_ => ThenTheErrorsContains("When not using service discovery DownstreamHostAndPorts must be set and not empty or Ocelot cannot find your service!"))
                .BDDfy();
        }

        [Fact]
        public void should_be_valid_if_using_service_discovery_and_no_host_and_ports()
        {
            var fileRoute = new FileRoute
            {
                DownstreamPathTemplate = "/test",
                UpstreamPathTemplate = "/test",
                ServiceName = "Lads"
            };

            this.Given(_ => GivenThe(fileRoute))
                .When(_ => WhenIValidate())
                .Then(_ => ThenTheResultIsValid())
                .BDDfy();
        }

        [Fact]
        public void should_be_valid_re_route_using_host_and_port_and_paths()
        {
            var fileRoute = new FileRoute
            {
                DownstreamPathTemplate = "/test",
                UpstreamPathTemplate = "/test",
                DownstreamHostAndPorts = new List<FileHostAndPort>
                {
                    new FileHostAndPort
                    {
                        Host = "localhost",
                        Port = 5000
                    }
                }
            };

            this.Given(_ => GivenThe(fileRoute))
                .When(_ => WhenIValidate())
                .Then(_ => ThenTheResultIsValid())
                .BDDfy();
        }

        [Fact]
        public void should_be_valid_if_specified_authentication_provider_is_registered()
        {
            const string key = "JwtLads";

            var fileRoute = new FileRoute
            {
                DownstreamPathTemplate = "/test",
                UpstreamPathTemplate = "/test",
                AuthenticationOptions = new FileAuthenticationOptions
                {
                    AuthenticationProviderKey = key
                },
                DownstreamHostAndPorts = new List<FileHostAndPort>
                {
                    new FileHostAndPort
                    {
                        Host = "localhost",
                        Port = 5000
                    }
                }
            };

            this.Given(_ => GivenThe(fileRoute))
                .And(_ => GivenAnAuthProvider(key))
                .When(_ => WhenIValidate())
                .Then(_ => ThenTheResultIsValid())
                .BDDfy();
        }

        [Theory]
        [InlineData("1.0")]
        [InlineData("1.1")]
        [InlineData("2.0")]
        [InlineData("1,0")]
        [InlineData("1,1")]
        [InlineData("2,0")]
        [InlineData("1")]
        [InlineData("2")]
        [InlineData("")]
        [InlineData(null)]
        public void should_be_valid_re_route_using_downstream_http_version(string version)
        {
            var fileRoute = new FileRoute
            {
                DownstreamPathTemplate = "/test",
                UpstreamPathTemplate = "/test",
                DownstreamHostAndPorts = new List<FileHostAndPort>
                {
                    new FileHostAndPort
                    {
                        Host = "localhost",
                        Port = 5000,
                    },
                },
                DownstreamHttpVersion = version,
            };

            this.Given(_ => GivenThe(fileRoute))
                .When(_ => WhenIValidate())
                .Then(_ => ThenTheResultIsValid())
                .BDDfy();
        }

        [Theory]
        [InlineData("retg1.1")]
        [InlineData("re2.0")]
        [InlineData("1,0a")]
        [InlineData("a1,1")]
        [InlineData("12,0")]
        [InlineData("asdf")]
        public void should_be_invalid_re_route_using_downstream_http_version(string version)
        {
            var fileRoute = new FileRoute
            {
                DownstreamPathTemplate = "/test",
                UpstreamPathTemplate = "/test",
                DownstreamHostAndPorts = new List<FileHostAndPort>
                {
                    new FileHostAndPort
                    {
                        Host = "localhost",
                        Port = 5000,
                    },
                },
                DownstreamHttpVersion = version,
            };

            this.Given(_ => GivenThe(fileRoute))
                .When(_ => WhenIValidate())
                .Then(_ => ThenTheResultIsInvalid())
                .And(_ => ThenTheErrorsContains("'Downstream Http Version' is not in the correct format."))
                .BDDfy();
        }

        private void GivenAnAuthProvider(string key)
        {
            var schemes = new List<AuthenticationScheme>
            {
                new AuthenticationScheme(key, key, typeof(FakeAutheHandler))
            };

            _authProvider
                .Setup(x => x.GetAllSchemesAsync())
                .ReturnsAsync(schemes);
        }

        private void ThenTheResultIsValid()
        {
            _result.IsValid.ShouldBeTrue();
        }

        private void GivenThe(FileRoute route)
        {
            _route = route;
        }

        private void WhenIValidate()
        {
            _result = _validator.Validate(_route);
        }

        private void ThenTheResultIsInvalid()
        {
            _result.IsValid.ShouldBeFalse();
        }

        private void ThenTheErrorsContains(string expected)
        {
            _result.Errors.ShouldContain(x => x.ErrorMessage == expected);
        }

        private class FakeAutheHandler : IAuthenticationHandler
        {
            public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
            {
                throw new System.NotImplementedException();
            }

            public Task<AuthenticateResult> AuthenticateAsync()
            {
                throw new System.NotImplementedException();
            }

            public Task ChallengeAsync(AuthenticationProperties properties)
            {
                throw new System.NotImplementedException();
            }

            public Task ForbidAsync(AuthenticationProperties properties)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
