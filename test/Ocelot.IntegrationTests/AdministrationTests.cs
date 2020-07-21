using IdentityServer4.AccessTokenValidation;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Ocelot.Administration;
using Ocelot.Cache;
using Ocelot.Configuration.File;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using TestStack.BDDfy;
using Ocelot.Configuration.ChangeTracking;
using Xunit;

namespace Ocelot.IntegrationTests
{
    public class AdministrationTests : IDisposable
    {
        private HttpClient _httpClient;
        private readonly HttpClient _httpClientTwo;
        private HttpResponseMessage _response;
        private IHost _builder;
        private IHostBuilder _webHostBuilder;
        private string _ocelotBaseUrl;
        private BearerToken _token;
        private IHostBuilder _webHostBuilderTwo;
        private IHost _builderTwo;
        private IHost _identityServerBuilder;
        private IHost _fooServiceBuilder;
        private IHost _barServiceBuilder;

        public AdministrationTests()
        {
            _httpClient = new HttpClient();
            _httpClientTwo = new HttpClient();
            _ocelotBaseUrl = "http://localhost:5000";
            _httpClient.BaseAddress = new Uri(_ocelotBaseUrl);
        }

        [Fact]
        public void should_return_response_401_with_call_re_routes_controller()
        {
            var configuration = new FileConfiguration();

            this.Given(x => GivenThereIsAConfiguration(configuration))
                .And(x => GivenOcelotIsRunning())
                .When(x => WhenIGetUrlOnTheApiGateway("/administration/configuration"))
                .Then(x => ThenTheStatusCodeShouldBe(HttpStatusCode.Unauthorized))
                .BDDfy();
        }

        [Fact]
        public void should_return_response_200_with_call_re_routes_controller()
        {
            var configuration = new FileConfiguration();

            this.Given(x => GivenThereIsAConfiguration(configuration))
                .And(x => GivenOcelotIsRunning())
                .And(x => GivenIHaveAnOcelotToken("/administration"))
                .And(x => GivenIHaveAddedATokenToMyRequest())
                .When(x => WhenIGetUrlOnTheApiGateway("/administration/configuration"))
                .Then(x => ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .BDDfy();
        }

        [Fact]
        public void should_return_response_200_with_call_re_routes_controller_using_base_url_added_in_file_config()
        {
            _httpClient = new HttpClient();
            _ocelotBaseUrl = "http://localhost:5011";
            _httpClient.BaseAddress = new Uri(_ocelotBaseUrl);

            var configuration = new FileConfiguration
            {
                GlobalConfiguration = new FileGlobalConfiguration
                {
                    BaseUrl = _ocelotBaseUrl
                }
            };

            this.Given(x => GivenThereIsAConfiguration(configuration))
                .And(x => GivenOcelotIsRunningWithNoWebHostBuilder(_ocelotBaseUrl))
                .And(x => GivenIHaveAnOcelotToken("/administration"))
                .And(x => GivenIHaveAddedATokenToMyRequest())
                .When(x => WhenIGetUrlOnTheApiGateway("/administration/configuration"))
                .Then(x => ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .BDDfy();
        }

        [Fact]
        public void should_be_able_to_use_token_from_ocelot_a_on_ocelot_b()
        {
            var configuration = new FileConfiguration();

            this.Given(x => GivenThereIsAConfiguration(configuration))
                .And(x => GivenIdentityServerSigningEnvironmentalVariablesAreSet())
                .And(x => GivenOcelotIsRunning())
                .And(x => GivenIHaveAnOcelotToken("/administration"))
                .And(x => GivenAnotherOcelotIsRunning("http://localhost:5017"))
                .When(x => WhenIGetUrlOnTheSecondOcelot("/administration/configuration"))
                .Then(x => ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .BDDfy();
        }

        [Fact]
        public void should_return_file_configuration()
        {
            var configuration = new FileConfiguration
            {
                GlobalConfiguration = new FileGlobalConfiguration
                {
                    RequestIdKey = "RequestId",
                    ServiceDiscoveryProvider = new FileServiceDiscoveryProvider
                    {
                        Scheme = "https",
                        Host = "127.0.0.1",
                    }
                },
                Routes = new List<FileRoute>()
                {
                    new FileRoute()
                    {
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "localhost",
                                Port = 80,
                            }
                        },
                        DownstreamScheme = "https",
                        DownstreamPathTemplate = "/",
                        UpstreamHttpMethod = new List<string> { "get" },
                        UpstreamPathTemplate = "/",
                        FileCacheOptions = new FileCacheOptions
                        {
                            TtlSeconds = 10,
                            Region = "Geoff"
                        }
                    },
                    new FileRoute()
                    {
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "localhost",
                                Port = 80,
                            }
                        },
                        DownstreamScheme = "https",
                        DownstreamPathTemplate = "/",
                        UpstreamHttpMethod = new List<string> { "get" },
                        UpstreamPathTemplate = "/test",
                        FileCacheOptions = new FileCacheOptions
                        {
                            TtlSeconds = 10,
                            Region = "Dave"
                        }
                    }
                }
            };

            this.Given(x => GivenThereIsAConfiguration(configuration))
                .And(x => GivenOcelotIsRunning())
                .And(x => GivenIHaveAnOcelotToken("/administration"))
                .And(x => GivenIHaveAddedATokenToMyRequest())
                .When(x => WhenIGetUrlOnTheApiGateway("/administration/configuration"))
                .Then(x => ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => ThenTheResponseShouldBe(configuration))
                .BDDfy();
        }

        [Fact]
        public void should_get_file_configuration_edit_and_post_updated_version()
        {
            var initialConfiguration = new FileConfiguration
            {
                GlobalConfiguration = new FileGlobalConfiguration
                {
                },
                Routes = new List<FileRoute>()
                {
                    new FileRoute()
                    {
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "localhost",
                                Port = 80,
                            }
                        },
                        DownstreamScheme = "https",
                        DownstreamPathTemplate = "/",
                        UpstreamHttpMethod = new List<string> { "get" },
                        UpstreamPathTemplate = "/"
                    },
                    new FileRoute()
                    {
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "localhost",
                                Port = 80,
                            }
                        },
                        DownstreamScheme = "https",
                        DownstreamPathTemplate = "/",
                        UpstreamHttpMethod = new List<string> { "get" },
                        UpstreamPathTemplate = "/test"
                    }
                },
            };

            var updatedConfiguration = new FileConfiguration
            {
                GlobalConfiguration = new FileGlobalConfiguration
                {
                },
                Routes = new List<FileRoute>()
                {
                    new FileRoute()
                    {
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "localhost",
                                Port = 80,
                            }
                        },
                        DownstreamScheme = "http",
                        DownstreamPathTemplate = "/geoffrey",
                        UpstreamHttpMethod = new List<string> { "get" },
                        UpstreamPathTemplate = "/"
                    },
                    new FileRoute()
                    {
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "123.123.123",
                                Port = 443,
                            }
                        },
                        DownstreamScheme = "https",
                        DownstreamPathTemplate = "/blooper/{productId}",
                        UpstreamHttpMethod = new List<string> { "post" },
                        UpstreamPathTemplate = "/test"
                    }
                }
            };

            this.Given(x => GivenThereIsAConfiguration(initialConfiguration))
                .And(x => GivenOcelotIsRunning())
                .And(x => GivenIHaveAnOcelotToken("/administration"))
                .And(x => GivenIHaveAddedATokenToMyRequest())
                .When(x => WhenIGetUrlOnTheApiGateway("/administration/configuration"))
                .When(x => WhenIPostOnTheApiGateway("/administration/configuration", updatedConfiguration))
                .Then(x => ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => ThenTheResponseShouldBe(updatedConfiguration))
                .When(x => WhenIGetUrlOnTheApiGateway("/administration/configuration"))
                .And(x => ThenTheResponseShouldBe(updatedConfiguration))
                .And(_ => ThenTheConfigurationIsSavedCorrectly(updatedConfiguration))
                .BDDfy();
        }

        [Fact]
        public void should_activate_change_token_when_configuration_is_updated()
        {
            var configuration = new FileConfiguration
            {
                GlobalConfiguration = new FileGlobalConfiguration(),
                Routes = new List<FileRoute>
                {
                    new FileRoute
                    {
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "localhost",
                                Port = 80,
                            },
                        },
                        DownstreamScheme = "https",
                        DownstreamPathTemplate = "/",
                        UpstreamHttpMethod = new List<string> { "get" },
                        UpstreamPathTemplate = "/",
                    },
                },
            };

            this.Given(x => GivenThereIsAConfiguration(configuration))
                .And(x => GivenOcelotIsRunning())
                .And(x => GivenIHaveAnOcelotToken("/administration"))
                .And(x => GivenIHaveAddedATokenToMyRequest())
                .When(x => WhenIPostOnTheApiGateway("/administration/configuration", configuration))
                .Then(x => ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => TheChangeTokenShouldBeActive())
                .And(x => ThenTheResponseShouldBe(configuration))
                .When(x => WhenIGetUrlOnTheApiGateway("/administration/configuration"))
                .And(x => ThenTheResponseShouldBe(configuration))
                .And(_ => ThenTheConfigurationIsSavedCorrectly(configuration))
                .BDDfy();
        }

        private void TheChangeTokenShouldBeActive()
        {
            _builder.Services.GetRequiredService<IOcelotConfigurationChangeTokenSource>().ChangeToken.HasChanged.ShouldBeTrue();
        }

        private void ThenTheConfigurationIsSavedCorrectly(FileConfiguration expected)
        {
            var ocelotJsonPath = $"{AppContext.BaseDirectory}ocelot.json";
            var resultText = File.ReadAllText(ocelotJsonPath);
            var expectedText = JsonConvert.SerializeObject(expected, Formatting.Indented);
            resultText.ShouldBe(expectedText);

            var environmentSpecificPath = $"{AppContext.BaseDirectory}/ocelot.Production.json";
            resultText = File.ReadAllText(environmentSpecificPath);
            expectedText = JsonConvert.SerializeObject(expected, Formatting.Indented);
            resultText.ShouldBe(expectedText);
        }

        [Fact]
        public void should_get_file_configuration_edit_and_post_updated_version_redirecting_route()
        {
            var fooPort = 47689;
            var barPort = 27654;

            var initialConfiguration = new FileConfiguration
            {
                Routes = new List<FileRoute>()
                {
                    new FileRoute()
                    {
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "localhost",
                                Port = fooPort,
                            }
                        },
                        DownstreamScheme = "http",
                        DownstreamPathTemplate = "/foo",
                        UpstreamHttpMethod = new List<string> { "get" },
                        UpstreamPathTemplate = "/foo"
                    }
                }
            };

            var updatedConfiguration = new FileConfiguration
            {
                GlobalConfiguration = new FileGlobalConfiguration
                {
                },
                Routes = new List<FileRoute>()
                {
                    new FileRoute()
                    {
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "localhost",
                                Port = barPort,
                            }
                        },
                        DownstreamScheme = "http",
                        DownstreamPathTemplate = "/bar",
                        UpstreamHttpMethod = new List<string> { "get" },
                        UpstreamPathTemplate = "/foo"
                    }
                }
            };

            this.Given(x => GivenThereIsAConfiguration(initialConfiguration))
                .And(x => GivenThereIsAFooServiceRunningOn($"http://localhost:{fooPort}"))
                .And(x => GivenThereIsABarServiceRunningOn($"http://localhost:{barPort}"))
                .And(x => GivenOcelotIsRunning())
                .And(x => WhenIGetUrlOnTheApiGateway("/foo"))
                .Then(x => ThenTheResponseBodyShouldBe("foo"))
                .And(x => GivenIHaveAnOcelotToken("/administration"))
                .And(x => GivenIHaveAddedATokenToMyRequest())
                .When(x => WhenIPostOnTheApiGateway("/administration/configuration", updatedConfiguration))
                .Then(x => ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => ThenTheResponseShouldBe(updatedConfiguration))
                .And(x => WhenIGetUrlOnTheApiGateway("/foo"))
                .Then(x => ThenTheResponseBodyShouldBe("bar"))
                .When(x => WhenIPostOnTheApiGateway("/administration/configuration", initialConfiguration))
                .Then(x => ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => ThenTheResponseShouldBe(initialConfiguration))
                .And(x => WhenIGetUrlOnTheApiGateway("/foo"))
                .Then(x => ThenTheResponseBodyShouldBe("foo"))
                .BDDfy();
        }

        [Fact]
        public void should_clear_region()
        {
            var initialConfiguration = new FileConfiguration
            {
                GlobalConfiguration = new FileGlobalConfiguration
                {
                },
                Routes = new List<FileRoute>()
                {
                    new FileRoute()
                    {
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "localhost",
                                Port = 80,
                            }
                        },
                        DownstreamScheme = "https",
                        DownstreamPathTemplate = "/",
                        UpstreamHttpMethod = new List<string> { "get" },
                        UpstreamPathTemplate = "/",
                        FileCacheOptions = new FileCacheOptions
                        {
                            TtlSeconds = 10
                        }
                    },
                    new FileRoute()
                    {
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "localhost",
                                Port = 80,
                            }
                        },
                        DownstreamScheme = "https",
                        DownstreamPathTemplate = "/",
                        UpstreamHttpMethod = new List<string> { "get" },
                        UpstreamPathTemplate = "/test",
                        FileCacheOptions = new FileCacheOptions
                        {
                            TtlSeconds = 10
                        }
                    }
                }
            };

            var regionToClear = "gettest";

            this.Given(x => GivenThereIsAConfiguration(initialConfiguration))
                .And(x => GivenOcelotIsRunning())
                .And(x => GivenIHaveAnOcelotToken("/administration"))
                .And(x => GivenIHaveAddedATokenToMyRequest())
                .When(x => WhenIDeleteOnTheApiGateway($"/administration/outputcache/{regionToClear}"))
                .Then(x => ThenTheStatusCodeShouldBe(HttpStatusCode.NoContent))
                .BDDfy();
        }

        [Fact]
        public void should_return_response_200_with_call_re_routes_controller_when_using_own_identity_server_to_secure_admin_area()
        {
            var configuration = new FileConfiguration();

            var identityServerRootUrl = "http://localhost:5123";

            Action<IdentityServerAuthenticationOptions> options = o =>
            {
                o.Authority = identityServerRootUrl;
                o.ApiName = "api";
                o.RequireHttpsMetadata = false;
                o.SupportedTokens = SupportedTokens.Both;
                o.ApiSecret = "secret";
            };

            this.Given(x => GivenThereIsAConfiguration(configuration))
                .And(x => GivenThereIsAnIdentityServerOn(identityServerRootUrl, "api"))
                .And(x => GivenOcelotIsRunningWithIdentityServerSettings(options))
                .And(x => GivenIHaveAToken(identityServerRootUrl))
                .And(x => GivenIHaveAddedATokenToMyRequest())
                .When(x => WhenIGetUrlOnTheApiGateway("/administration/configuration"))
                .Then(x => ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .BDDfy();
        }

        private void GivenIHaveAToken(string url)
        {
            var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id", "api"),
                new KeyValuePair<string, string>("client_secret", "secret"),
                new KeyValuePair<string, string>("scope", "api"),
                new KeyValuePair<string, string>("username", "test"),
                new KeyValuePair<string, string>("password", "test"),
                new KeyValuePair<string, string>("grant_type", "password")
            };
            var content = new FormUrlEncodedContent(formData);

            using (var httpClient = new HttpClient())
            {
                var response = httpClient.PostAsync($"{url}/connect/token", content).Result;
                var responseContent = response.Content.ReadAsStringAsync().Result;
                response.EnsureSuccessStatusCode();
                _token = JsonConvert.DeserializeObject<BearerToken>(responseContent);
            }
        }

        private void GivenThereIsAnIdentityServerOn(string url, string apiName)
        {
            _identityServerBuilder = Host.CreateDefaultBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder.UseUrls(url)
                    .UseKestrel()
                    .UseContentRoot(Directory.GetCurrentDirectory())
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
                            Description = apiName,
                            Enabled = true,
                            DisplayName = apiName,
                            Scopes = new List<Scope>()
                            {
                                new Scope(apiName),
                            },
                        },
                        })
                        .AddInMemoryClients(new List<Client>
                        {
                        new Client
                        {
                            ClientId = apiName,
                            AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                            ClientSecrets = new List<Secret> { new Secret("secret".Sha256()) },
                            AllowedScopes = new List<string> { apiName },
                            AccessTokenType = AccessTokenType.Jwt,
                            Enabled = true
                        },
                        })
                        .AddTestUsers(new List<TestUser>
                        {
                        new TestUser
                        {
                            Username = "test",
                            Password = "test",
                            SubjectId = "1231231"
                        },
                        });
                    })
                    .Configure(app =>
                    {
                        app.UseIdentityServer();
                    }
                    );
                }).Build();

            _identityServerBuilder.Start();

            using (var httpClient = new HttpClient())
            {
                var response = httpClient.GetAsync($"{url}/.well-known/openid-configuration").Result;
                response.EnsureSuccessStatusCode();
            }
        }

        private void GivenAnotherOcelotIsRunning(string baseUrl)
        {
            _httpClientTwo.BaseAddress = new Uri(baseUrl);

            _webHostBuilderTwo = Host.CreateDefaultBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder.UseUrls(baseUrl)
                    .UseKestrel()
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        config.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath);
                        var env = hostingContext.HostingEnvironment;
                        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: false);
                        config.AddJsonFile("ocelot.json", false, false);
                        config.AddEnvironmentVariables();
                    })
                    .ConfigureServices(x =>
                    {
                        x.AddMvc(option => option.EnableEndpointRouting = false);
                        x.AddOcelot()
                       .AddAdministration("/administration", "secret");
                    })
                    .Configure(app =>
                    {
                        app.UseOcelot().Wait();
                    });
                });

            _builderTwo = _webHostBuilderTwo.Build();

            _builderTwo.Start();
        }

        private void GivenIdentityServerSigningEnvironmentalVariablesAreSet()
        {
            Environment.SetEnvironmentVariable("OCELOT_CERTIFICATE", "idsrv3test.pfx");
            Environment.SetEnvironmentVariable("OCELOT_CERTIFICATE_PASSWORD", "idsrv3test");
        }

        private void WhenIGetUrlOnTheSecondOcelot(string url)
        {
            _httpClientTwo.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token.AccessToken);
            _response = _httpClientTwo.GetAsync(url).Result;
        }

        private void WhenIPostOnTheApiGateway(string url, FileConfiguration updatedConfiguration)
        {
            var json = JsonConvert.SerializeObject(updatedConfiguration);
            var content = new StringContent(json);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            _response = _httpClient.PostAsync(url, content).Result;
        }

        private void ThenTheResponseShouldBe(List<string> expected)
        {
            var content = _response.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<Regions>(content);
            result.Value.ShouldBe(expected);
        }

        private void ThenTheResponseBodyShouldBe(string expected)
        {
            var content = _response.Content.ReadAsStringAsync().Result;
            content.ShouldBe(expected);
        }

        private void ThenTheResponseShouldBe(FileConfiguration expecteds)
        {
            var response = JsonConvert.DeserializeObject<FileConfiguration>(_response.Content.ReadAsStringAsync().Result);

            response.GlobalConfiguration.RequestIdKey.ShouldBe(expecteds.GlobalConfiguration.RequestIdKey);
            response.GlobalConfiguration.ServiceDiscoveryProvider.Scheme.ShouldBe(expecteds.GlobalConfiguration.ServiceDiscoveryProvider.Scheme);
            response.GlobalConfiguration.ServiceDiscoveryProvider.Host.ShouldBe(expecteds.GlobalConfiguration.ServiceDiscoveryProvider.Host);
            response.GlobalConfiguration.ServiceDiscoveryProvider.Port.ShouldBe(expecteds.GlobalConfiguration.ServiceDiscoveryProvider.Port);

            for (var i = 0; i < response.Routes.Count; i++)
            {
                for (var j = 0; j < response.Routes[i].DownstreamHostAndPorts.Count; j++)
                {
                    var result = response.Routes[i].DownstreamHostAndPorts[j];
                    var expected = expecteds.Routes[i].DownstreamHostAndPorts[j];
                    result.Host.ShouldBe(expected.Host);
                    result.Port.ShouldBe(expected.Port);
                }

                response.Routes[i].DownstreamPathTemplate.ShouldBe(expecteds.Routes[i].DownstreamPathTemplate);
                response.Routes[i].DownstreamScheme.ShouldBe(expecteds.Routes[i].DownstreamScheme);
                response.Routes[i].UpstreamPathTemplate.ShouldBe(expecteds.Routes[i].UpstreamPathTemplate);
                response.Routes[i].UpstreamHttpMethod.ShouldBe(expecteds.Routes[i].UpstreamHttpMethod);
            }
        }

        private void GivenIHaveAddedATokenToMyRequest()
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token.AccessToken);
        }

        private void GivenIHaveAnOcelotToken(string adminPath)
        {
            var tokenUrl = $"{adminPath}/connect/token";
            var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id", "admin"),
                new KeyValuePair<string, string>("client_secret", "secret"),
                new KeyValuePair<string, string>("scope", "admin"),
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            };
            var content = new FormUrlEncodedContent(formData);

            var response = _httpClient.PostAsync(tokenUrl, content).Result;
            var responseContent = response.Content.ReadAsStringAsync().Result;
            response.EnsureSuccessStatusCode();
            _token = JsonConvert.DeserializeObject<BearerToken>(responseContent);
            var configPath = $"{adminPath}/.well-known/openid-configuration";
            response = _httpClient.GetAsync(configPath).Result;
            response.EnsureSuccessStatusCode();
        }

        private void GivenOcelotIsRunningWithIdentityServerSettings(Action<IdentityServerAuthenticationOptions> configOptions)
        {
            _webHostBuilder = Host.CreateDefaultBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder.UseUrls(_ocelotBaseUrl)
                    .UseKestrel()
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        config.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath);
                        var env = hostingContext.HostingEnvironment;
                        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: false);
                        config.AddJsonFile("ocelot.json", false, false);
                        config.AddEnvironmentVariables();
                    })
                    .ConfigureServices(x =>
                    {
                        x.AddMvc(option => option.EnableEndpointRouting = false);
                        x.AddSingleton(_webHostBuilder);
                        x.AddOcelot()
                        .AddAdministration("/administration", configOptions);
                    })
                    .Configure(app =>
                    {
                        app.UseOcelot().Wait();
                    });
                });

            _builder = _webHostBuilder.Build();

            _builder.Start();
        }

        private void GivenOcelotIsRunning()
        {
            _webHostBuilder = Host.CreateDefaultBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder.UseUrls(_ocelotBaseUrl)
                    .UseKestrel()
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        config.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath);
                        var env = hostingContext.HostingEnvironment;
                        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: false);
                        config.AddJsonFile("ocelot.json", false, false);
                        config.AddEnvironmentVariables();
                    })
                    .ConfigureServices(x =>
                    {
                        x.AddMvc(s => s.EnableEndpointRouting = false);
                        x.AddOcelot()
                        .AddAdministration("/administration", "secret");
                    })
                    .Configure(app =>
                    {
                        app.UseOcelot().Wait();
                    });
                });

            _builder = _webHostBuilder.Build();

            _builder.Start();
        }

        private void GivenOcelotIsRunningWithNoWebHostBuilder(string baseUrl)
        {
            _webHostBuilder = Host.CreateDefaultBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder.UseUrls(_ocelotBaseUrl)
                    .UseKestrel()
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        config.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath);
                        var env = hostingContext.HostingEnvironment;
                        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: false);
                        config.AddJsonFile("ocelot.json", false, false);
                        config.AddEnvironmentVariables();
                    })
                    .ConfigureServices(x =>
                    {
                        x.AddMvc(option => option.EnableEndpointRouting = false);
                        x.AddSingleton(_webHostBuilder);
                        x.AddOcelot()
                        .AddAdministration("/administration", "secret");
                    })
                    .Configure(app =>
                    {
                        app.UseOcelot().Wait();
                    });
                });        
            
            _builder = _webHostBuilder.Build();

            _builder.Start();
        }

        private void GivenThereIsAConfiguration(FileConfiguration fileConfiguration)
        {
            var configurationPath = $"{Directory.GetCurrentDirectory()}/ocelot.json";

            var jsonConfiguration = JsonConvert.SerializeObject(fileConfiguration);

            if (File.Exists(configurationPath))
            {
                File.Delete(configurationPath);
            }

            File.WriteAllText(configurationPath, jsonConfiguration);

            var text = File.ReadAllText(configurationPath);

            configurationPath = $"{AppContext.BaseDirectory}/ocelot.json";

            if (File.Exists(configurationPath))
            {
                File.Delete(configurationPath);
            }

            File.WriteAllText(configurationPath, jsonConfiguration);

            text = File.ReadAllText(configurationPath);
        }

        private void WhenIGetUrlOnTheApiGateway(string url)
        {
            _response = _httpClient.GetAsync(url).Result;
        }

        private void WhenIDeleteOnTheApiGateway(string url)
        {
            _response = _httpClient.DeleteAsync(url).Result;
        }

        private void ThenTheStatusCodeShouldBe(HttpStatusCode expectedHttpStatusCode)
        {
            _response.StatusCode.ShouldBe(expectedHttpStatusCode);
        }

        public void Dispose()
        {
            Environment.SetEnvironmentVariable("OCELOT_CERTIFICATE", "");
            Environment.SetEnvironmentVariable("OCELOT_CERTIFICATE_PASSWORD", "");
            _builder?.Dispose();
            _httpClient?.Dispose();
            _identityServerBuilder?.Dispose();
        }

        private void GivenThereIsAFooServiceRunningOn(string baseUrl)
        {
            _fooServiceBuilder = Host.CreateDefaultBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder.UseUrls(baseUrl)
                    .UseKestrel()
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseIISIntegration()
                    .Configure(app =>
                    {
                        app.UsePathBase("/foo");
                        app.Run(async context =>
                        {
                            context.Response.StatusCode = 200;
                            await context.Response.WriteAsync("foo");
                        });
                    });
                }).Build();
            
            _fooServiceBuilder.Start();
        }

        private void GivenThereIsABarServiceRunningOn(string baseUrl)
        {
            _barServiceBuilder = Host.CreateDefaultBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder.UseUrls(baseUrl)
                    .UseKestrel()
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseIISIntegration()
                    .Configure(app =>
                    {
                        app.UsePathBase("/bar");
                        app.Run(async context =>
                        {
                            context.Response.StatusCode = 200;
                            await context.Response.WriteAsync("bar");
                        });
                    });
                }).Build();

            _barServiceBuilder.Start();
        }
    }
}
