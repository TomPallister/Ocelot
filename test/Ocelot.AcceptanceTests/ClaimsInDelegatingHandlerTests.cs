using IdentityServer4.AccessTokenValidation;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Ocelot.Configuration.File;
using Ocelot.Requester;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.AcceptanceTests
{
    public class ClaimsInDelegatingHandlerTests : IDisposable
    {
        private IWebHost _servicebuilder;
        private IWebHost _identityServerBuilder;
        private readonly Steps _steps;
        private Action<IdentityServerAuthenticationOptions> _options;
        private string _identityServerRootUrl;

        public ClaimsInDelegatingHandlerTests()
        {
            var identityServerPort = RandomPortFinder.GetRandomPort();
            _identityServerRootUrl = $"http://localhost:{identityServerPort}";
            _steps = new Steps();
            _options = o =>
            {
                o.Authority = _identityServerRootUrl;
                o.ApiName = "api";
                o.RequireHttpsMetadata = false;
                o.SupportedTokens = SupportedTokens.Both;
                o.ApiSecret = "secret";
            };
        }

        [Fact]
        public void should_expose_claims_in_global_delegating_handler()
        {
            var user = new TestUser()
            {
                Username = "test",
                Password = "test",
                SubjectId = "registered|1231231",
            };

            int port = RandomPortFinder.GetRandomPort();

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                   {
                       new FileRoute
                       {
                           DownstreamPathTemplate = "/users/{userId}",
                           DownstreamHostAndPorts = new List<FileHostAndPort>
                           {
                               new FileHostAndPort
                               {
                                   Host = "localhost",
                                   Port = port,
                               },
                           },
                           DownstreamScheme = "http",
                           UpstreamPathTemplate = "/users/{userId}",
                           UpstreamHttpMethod = new List<string> { "Get" },
                           AuthenticationOptions = new FileAuthenticationOptions
                           {
                               AuthenticationProviderKey = "Test",
                               AllowedScopes = new List<string>
                               {
                                   "openid", "offline_access", "api",
                               },
                           },
                       },
                   },
            };

            this.Given(x => x.GivenThereIsAnIdentityServerOn(_identityServerRootUrl, "api", AccessTokenType.Jwt, user, null))
                .And(x => _steps.GivenIHaveAToken(_identityServerRootUrl))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunningWithGlobalHandlerRegisteredInDi<FakeHandler>(_options, "Test"))
                .And(x => _steps.GivenIHaveAddedATokenToMyRequest())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/users/1231231"))
                .Then(x => ThenClaimsShouldBeExposedInDelegatingHandler())
                .BDDfy();
        }

        [Fact]
        public void should_expose_claims_in_route_specific_delegating_handler()
        {
            var user = new TestUser()
            {
                Username = "test",
                Password = "test",
                SubjectId = "registered|1231231",
            };

            int port = RandomPortFinder.GetRandomPort();

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                   {
                       new FileRoute
                       {
                           DownstreamPathTemplate = "/users/{userId}",
                           DownstreamHostAndPorts = new List<FileHostAndPort>
                           {
                               new FileHostAndPort
                               {
                                   Host = "localhost",
                                   Port = port,
                               },
                           },
                           DownstreamScheme = "http",
                           UpstreamPathTemplate = "/users/{userId}",
                           UpstreamHttpMethod = new List<string> { "Get" },
                           AuthenticationOptions = new FileAuthenticationOptions
                           {
                               AuthenticationProviderKey = "Test",
                               AllowedScopes = new List<string>
                               {
                                   "openid", "offline_access", "api",
                               },
                           },
                           DelegatingHandlers = new List<string>()
                           {
                               "FakeHandler",
                           },
                       },
                   },
            };

            this.Given(x => x.GivenThereIsAnIdentityServerOn(_identityServerRootUrl, "api", AccessTokenType.Jwt, user, null))
                .And(x => _steps.GivenIHaveAToken(_identityServerRootUrl))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunningWithSpecificHandlerRegisteredInDi<FakeHandler>(_options, "Test"))
                .And(x => _steps.GivenIHaveAddedATokenToMyRequest())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/users/1231231"))
                .Then(x => ThenClaimsShouldBeExposedInDelegatingHandler())
                .BDDfy();
        }

        [Fact]
        public void should_update_claims_in_global_delegating_handler()
        {
            var user = new TestUser()
            {
                Username = "test",
                Password = "test",
                SubjectId = "registered|1111",
            };
            var user2 = new TestUser()
            {
                Username = "test2",
                Password = "test",
                SubjectId = "registered|2222",
            };

            int port = RandomPortFinder.GetRandomPort();

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                   {
                       new FileRoute
                       {
                           DownstreamPathTemplate = "/users/{userId}",
                           DownstreamHostAndPorts = new List<FileHostAndPort>
                           {
                               new FileHostAndPort
                               {
                                   Host = "localhost",
                                   Port = port,
                               },
                           },
                           DownstreamScheme = "http",
                           UpstreamPathTemplate = "/users/{userId}",
                           UpstreamHttpMethod = new List<string> { "Get" },
                           AuthenticationOptions = new FileAuthenticationOptions
                           {
                               AuthenticationProviderKey = "Test",
                               AllowedScopes = new List<string>
                               {
                                   "openid", "offline_access", "api",
                               },
                           },
                       },
                   },
            };

            this.Given(x => x.GivenThereIsAnIdentityServerOn(_identityServerRootUrl, "api", AccessTokenType.Jwt, user, user2))
                .And(x => _steps.GivenIHaveAToken(_identityServerRootUrl))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunningWithGlobalHandlerRegisteredInDi<FakeHandler>(_options, "Test"))
                .And(x => _steps.GivenIHaveAddedATokenToMyRequest())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/users/1111"))
                .Then(x => ThenClaimsShouldBeExposedInDelegatingHandler())
                .And(x => ThenClaimSubjectShouldBe("registered|1111"))
                .And(x => _steps.GivenIHaveAToken(_identityServerRootUrl, "test2"))
                .And(x => _steps.GivenIHaveAddedATokenToMyRequest())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/users/2222"))
                .Then(x => ThenClaimsShouldBeExposedInDelegatingHandler())
                .And(x => ThenClaimSubjectShouldBe("registered|2222"))
                .BDDfy();
        }

        [Fact]
        public void should_update_claims_in_route_specific_delegating_handler()
        {
            var user = new TestUser()
            {
                Username = "test",
                Password = "test",
                SubjectId = "registered|1111",
            };
            var user2 = new TestUser()
            {
                Username = "test2",
                Password = "test",
                SubjectId = "registered|2222",
            };

            int port = RandomPortFinder.GetRandomPort();

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                   {
                       new FileRoute
                       {
                           DownstreamPathTemplate = "/users/{userId}",
                           DownstreamHostAndPorts = new List<FileHostAndPort>
                           {
                               new FileHostAndPort
                               {
                                   Host = "localhost",
                                   Port = port,
                               },
                           },
                           DownstreamScheme = "http",
                           UpstreamPathTemplate = "/users/{userId}",
                           UpstreamHttpMethod = new List<string> { "Get" },
                           AuthenticationOptions = new FileAuthenticationOptions
                           {
                               AuthenticationProviderKey = "Test",
                               AllowedScopes = new List<string>
                               {
                                   "openid", "offline_access", "api",
                               },
                           },
                           DelegatingHandlers = new List<string>()
                           {
                               "OtherFakeHandler",
                               "FakeHandler",
                           },
                       },
                   },
            };

            this.Given(x => x.GivenThereIsAnIdentityServerOn(_identityServerRootUrl, "api", AccessTokenType.Jwt, user, user2))
                .And(x => _steps.GivenIHaveAToken(_identityServerRootUrl, "test"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunningWithSpecficHandlersRegisteredInDi<OtherFakeHandler, FakeHandler>(_options, "Test"))
                .And(x => _steps.GivenIHaveAddedATokenToMyRequest())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/users/1111"))
                .Then(x => ThenClaimsShouldBeExposedInDelegatingHandler())
                .And(x => ThenClaimSubjectShouldBe("registered|1111"))
                .And(x => _steps.GivenIHaveAToken(_identityServerRootUrl, "test2"))
                .And(x => _steps.GivenIHaveAddedATokenToMyRequest())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/users/2222"))
                .Then(x => ThenClaimsShouldBeExposedInDelegatingHandler())
                .And(x => ThenClaimSubjectShouldBe("registered|2222"))
                .BDDfy();
        }

        private void ThenClaimsShouldBeExposedInDelegatingHandler()
        {
            FakeHandler.ClaimsExist.ShouldBeTrue();
        }

        private void ThenClaimSubjectShouldBe(string subject)
        {
            FakeHandler.ClaimSubject.ShouldBe(subject);
        }

        private void GivenThereIsAnIdentityServerOn(string url, string apiName, AccessTokenType tokenType, TestUser user, TestUser user2)
        {
            _identityServerBuilder = new WebHostBuilder()
                .UseUrls(url)
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseUrls(url)
                .ConfigureServices(services =>
                {
                    services.AddLogging();
                    services.AddIdentityServer()
                        .AddDeveloperSigningCredential()
                        .AddInMemoryApiResources(new List<ApiResource>
                        {
                            new ApiResource
                            {
                                Name = apiName,
                                Description = "My API",
                                Enabled = true,
                                DisplayName = "test",
                                Scopes = new List<Scope>()
                                {
                                    new Scope("api"),
                                    new Scope("openid"),
                                    new Scope("offline_access"),
                                },
                                ApiSecrets = new List<Secret>()
                                {
                                    new Secret
                                    {
                                        Value = "secret".Sha256(),
                                    },
                                },
                                UserClaims = new List<string>()
                                {
                                    "CustomerId", "LocationId", "UserType", "UserId",
                                },
                            },
                        })
                        .AddInMemoryClients(new List<Client>
                        {
                            new Client
                            {
                                ClientId = "client",
                                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                                ClientSecrets = new List<Secret> {new Secret("secret".Sha256())},
                                AllowedScopes = new List<string> { apiName, "openid", "offline_access" },
                                AccessTokenType = tokenType,
                                Enabled = true,
                                RequireClientSecret = false,
                            },
                        })
                        .AddTestUsers(new List<TestUser>
                        {
                            user, user2,
                        });
                })
                .Configure(app =>
                {
                    app.UseIdentityServer();
                })
                .Build();

            _identityServerBuilder.Start();

            _steps.VerifyIdentiryServerStarted(url);
        }

        public void Dispose()
        {
            _servicebuilder?.Dispose();
            _steps.Dispose();
            _identityServerBuilder?.Dispose();
        }

        private class FakeHandler : DelegatingHandler, IDelegatingHandlerWithHttpContext
        {
            public static bool ClaimsExist { get; private set; } 
            public static string ClaimSubject { get; private set; }

            public HttpContext HttpContext 
            { 
                get; 
                set; 
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                ClaimsExist = (bool)HttpContext?.User?.Claims.Any(); 
                if (ClaimsExist)
                {
                    ClaimSubject = HttpContext?.User?.Claims.SingleOrDefault(c => c.Type == "sub")?.Value;
                }

                return await base.SendAsync(request, cancellationToken);
            }
        }

        private class OtherFakeHandler : DelegatingHandler
        {
            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return await base.SendAsync(request, cancellationToken);
            }
        }
    }
}
