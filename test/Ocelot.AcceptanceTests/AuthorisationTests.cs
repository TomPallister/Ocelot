namespace Ocelot.AcceptanceTests
{
    using IdentityServer4.AccessTokenValidation;
    using IdentityServer4.Models;
    using IdentityServer4.Test;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Ocelot.Configuration.File;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Security.Claims;
    using TestStack.BDDfy;
    using Xunit;

    public class AuthorisationTests : IDisposable
    {
        private IWebHost _identityServerBuilder;
        private readonly Steps _steps;
        private readonly Action<IdentityServerAuthenticationOptions> _options;
        private string _identityServerRootUrl;
        private readonly ServiceHandler _serviceHandler;

        public AuthorisationTests()
        {
            _serviceHandler = new ServiceHandler();
            _steps = new Steps();
            var identityServerPort = RandomPortFinder.GetRandomPort();
            _identityServerRootUrl = $"http://localhost:{identityServerPort}";
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
        public void should_return_response_200_authorising_route()
        {
            int port = RandomPortFinder.GetRandomPort();

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                   {
                       new FileRoute
                       {
                           DownstreamPathTemplate = "/",
                           DownstreamHostAndPorts = new List<FileHostAndPort>
                           {
                               new FileHostAndPort
                               {
                                   Host = "localhost",
                                   Port = port,
                               }
                           },
                           DownstreamScheme = "http",
                           UpstreamPathTemplate = "/",
                           UpstreamHttpMethod = new List<string> { "Get" },
                           AuthenticationOptions = new FileAuthenticationOptions
                           {
                               AuthenticationProviderKey = "Test"
                           },
                           AddHeadersToRequest =
                           {
                               {"CustomerId", "Claims[CustomerId] > value"},
                               {"LocationId", "Claims[LocationId] > value"},
                               {"UserType", "Claims[sub] > value[0] > |"},
                               {"UserId", "Claims[sub] > value[1] > |"}
                           },
                           AddClaimsToRequest =
                           {
                               {"CustomerId", "Claims[CustomerId] > value"},
                               {"UserType", "Claims[sub] > value[0] > |"},
                               {"UserId", "Claims[sub] > value[1] > |"}
                           },
                           RouteClaimsRequirement =
                           {
                               {"UserType", "registered"}
                           }
                       }
                   }
            };

            this.Given(x => x.GivenThereIsAnIdentityServerOn(_identityServerRootUrl, "api", AccessTokenType.Jwt))
                .And(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", 200, "Hello from Laura"))
                .And(x => _steps.GivenIHaveAToken(_identityServerRootUrl))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning(_options, "Test"))
                .And(x => _steps.GivenIHaveAddedATokenToMyRequest())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from Laura"))
                .BDDfy();
        }

        [Fact]
        public void should_return_response_403_authorising_route()
        {
            int port = RandomPortFinder.GetRandomPort();

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                   {
                       new FileRoute
                       {
                           DownstreamPathTemplate = "/",
                           DownstreamHostAndPorts = new List<FileHostAndPort>
                           {
                               new FileHostAndPort
                               {
                                   Host = "localhost",
                                   Port = port,
                               }
                           },
                           DownstreamScheme = "http",
                           UpstreamPathTemplate = "/",
                           UpstreamHttpMethod = new List<string> { "Get" },
                           AuthenticationOptions = new FileAuthenticationOptions
                           {
                               AuthenticationProviderKey = "Test"
                           },
                           AddHeadersToRequest =
                           {
                               {"CustomerId", "Claims[CustomerId] > value"},
                               {"LocationId", "Claims[LocationId] > value"},
                               {"UserType", "Claims[sub] > value[0] > |"},
                               {"UserId", "Claims[sub] > value[1] > |"}
                           },
                           AddClaimsToRequest =
                           {
                               {"CustomerId", "Claims[CustomerId] > value"},
                               {"UserId", "Claims[sub] > value[1] > |"}
                           },
                           RouteClaimsRequirement =
                           {
                               {"UserType", "registered"}
                           }
                       }
                   }
            };

            this.Given(x => x.GivenThereIsAnIdentityServerOn(_identityServerRootUrl, "api", AccessTokenType.Jwt))
                .And(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", 200, "Hello from Laura"))
                .And(x => _steps.GivenIHaveAToken(_identityServerRootUrl))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning(_options, "Test"))
                .And(x => _steps.GivenIHaveAddedATokenToMyRequest())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.Forbidden))
                .BDDfy();
        }

        [Fact]
        public void should_return_response_200_using_identity_server_with_allowed_scope()
        {
            int port = RandomPortFinder.GetRandomPort();

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                   {
                       new FileRoute
                       {
                           DownstreamPathTemplate = "/",
                           DownstreamHostAndPorts = new List<FileHostAndPort>
                           {
                               new FileHostAndPort
                               {
                                   Host = "localhost",
                                   Port = port,
                               }
                           },
                           DownstreamScheme = "http",
                           UpstreamPathTemplate = "/",
                           UpstreamHttpMethod = new List<string> { "Get" },
                           AuthenticationOptions = new FileAuthenticationOptions
                           {
                               AuthenticationProviderKey = "Test",
                               AllowedScopes = new List<string>{ "api", "api.readOnly", "openid", "offline_access" },
                           },
                       }
                   }
            };

            this.Given(x => x.GivenThereIsAnIdentityServerOn(_identityServerRootUrl, "api", AccessTokenType.Jwt))
                .And(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", 200, "Hello from Laura"))
                .And(x => _steps.GivenIHaveATokenForApiReadOnlyScope(_identityServerRootUrl))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning(_options, "Test"))
                .And(x => _steps.GivenIHaveAddedATokenToMyRequest())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .BDDfy();
        }

        [Fact]
        public void should_return_response_403_using_identity_server_with_scope_not_allowed()
        {
            int port = RandomPortFinder.GetRandomPort();

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                   {
                       new FileRoute
                       {
                           DownstreamPathTemplate = "/",
                           DownstreamHostAndPorts = new List<FileHostAndPort>
                           {
                               new FileHostAndPort
                               {
                                   Host = "localhost",
                                   Port = port,
                               }
                           },
                           DownstreamScheme = "http",
                           UpstreamPathTemplate = "/",
                           UpstreamHttpMethod = new List<string> { "Get" },
                           AuthenticationOptions = new FileAuthenticationOptions
                           {
                               AuthenticationProviderKey = "Test",
                               AllowedScopes = new List<string>{ "api", "openid", "offline_access" },
                           },
                       }
                   }
            };

            this.Given(x => x.GivenThereIsAnIdentityServerOn(_identityServerRootUrl, "api", AccessTokenType.Jwt))
                .And(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", 200, "Hello from Laura"))
                .And(x => _steps.GivenIHaveATokenForApiReadOnlyScope(_identityServerRootUrl))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning(_options, "Test"))
                .And(x => _steps.GivenIHaveAddedATokenToMyRequest())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.Forbidden))
                .BDDfy();
        }

        [Fact]
        public void should_fix_issue_240()
        {
            int port = RandomPortFinder.GetRandomPort();

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                   {
                       new FileRoute
                       {
                           DownstreamPathTemplate = "/",
                           DownstreamHostAndPorts = new List<FileHostAndPort>
                           {
                               new FileHostAndPort
                               {
                                   Host = "localhost",
                                   Port = port,
                               }
                           },
                           DownstreamScheme = "http",
                           UpstreamPathTemplate = "/",
                           UpstreamHttpMethod = new List<string> { "Get" },
                           AuthenticationOptions = new FileAuthenticationOptions
                           {
                               AuthenticationProviderKey = "Test"
                           },
                           RouteClaimsRequirement =
                           {
                               {"Role", "User"}
                           }
                       }
                   }
            };

            var users = new List<TestUser>
            {
                new TestUser
                {
                    Username = "test",
                    Password = "test",
                    SubjectId = "registered|1231231",
                    Claims = new List<Claim>
                    {
                        new Claim("Role", "AdminUser"),
                        new Claim("Role", "User")
                    },
                }
            };

            this.Given(x => x.GivenThereIsAnIdentityServerOn(_identityServerRootUrl, "api", AccessTokenType.Jwt, users))
                .And(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", 200, "Hello from Laura"))
                .And(x => _steps.GivenIHaveAToken(_identityServerRootUrl))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning(_options, "Test"))
                .And(x => _steps.GivenIHaveAddedATokenToMyRequest())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from Laura"))
                .BDDfy();
        }

        private void GivenThereIsAServiceRunningOn(string url, int statusCode, string responseBody)
        {
            _serviceHandler.GivenThereIsAServiceRunningOn(url, async context =>
            {
                context.Response.StatusCode = statusCode;
                await context.Response.WriteAsync(responseBody);
            });
        }

        private void GivenThereIsAnIdentityServerOn(string url, string apiName, AccessTokenType tokenType)
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
                                    new Scope("api.readOnly"),
                                    new Scope("openid"),
                                    new Scope("offline_access")
                                },
                                ApiSecrets = new List<Secret>()
                                {
                                    new Secret
                                    {
                                        Value = "secret".Sha256()
                                    }
                                },
                                UserClaims = new List<string>()
                                {
                                    "CustomerId", "LocationId", "UserType", "UserId"
                                }
                            },
                        })
                        .AddInMemoryClients(new List<Client>
                        {
                            new Client
                            {
                                ClientId = "client",
                                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                                ClientSecrets = new List<Secret> {new Secret("secret".Sha256())},
                                AllowedScopes = new List<string> { apiName, "api.readOnly", "openid", "offline_access" },
                                AccessTokenType = tokenType,
                                Enabled = true,
                                RequireClientSecret = false
                            }
                        })
                        .AddTestUsers(new List<TestUser>
                        {
                            new TestUser
                            {
                                Username = "test",
                                Password = "test",
                                SubjectId = "registered|1231231",
                                Claims = new List<Claim>
                                {
                                   new Claim("CustomerId", "123"),
                                   new Claim("LocationId", "321")
                                }
                            }
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

        private void GivenThereIsAnIdentityServerOn(string url, string apiName, AccessTokenType tokenType, List<TestUser> users)
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
                                    new Scope("api.readOnly"),
                                    new Scope("openid"),
                                    new Scope("offline_access"),
                                },
                                ApiSecrets = new List<Secret>()
                                {
                                    new Secret
                                    {
                                        Value = "secret".Sha256()
                                    }
                                },
                                UserClaims = new List<string>()
                                {
                                    "CustomerId", "LocationId", "UserType", "UserId", "Role"
                                }
                            },
                        })
                        .AddInMemoryClients(new List<Client>
                        {
                            new Client
                            {
                                ClientId = "client",
                                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                                ClientSecrets = new List<Secret> {new Secret("secret".Sha256())},
                                AllowedScopes = new List<string> { apiName, "api.readOnly", "openid", "offline_access" },
                                AccessTokenType = tokenType,
                                Enabled = true,
                                RequireClientSecret = false,
                            }
                        })
                        .AddTestUsers(users);
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
            _serviceHandler?.Dispose();
            _steps.Dispose();
            _identityServerBuilder?.Dispose();
        }
    }
}
