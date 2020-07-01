using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Ocelot.Authentication.Extensions.ApiKey;
using Ocelot.Configuration.File;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.AcceptanceTests
{
    public class ApiKeyTests : IDisposable
    {
        private readonly Steps _steps;

        private string _apiServerRootUrl;
        private string _downstreamServicePath = "/";
        private string _downstreamServiceHost = "localhost";
        private string _downstreamServiceScheme = "http";
        private string _downstreamServiceUrl = "http://localhost:";

        private string _apiKeyValidationPath = "validateapikey";

        private readonly ServiceHandler _serviceHandler;

        private readonly Action<ApiKeyAuthenticationOptions> _getOptions;
        private readonly Action<ApiKeyAuthenticationOptions> _postOptions;

        public ApiKeyTests()
        {
            _steps = new Steps();
            _serviceHandler = new ServiceHandler();
            var mockValidationApiPort = RandomPortFinder.GetRandomPort();
            _apiServerRootUrl = $"http://localhost:{mockValidationApiPort}";

            _getOptions = o =>
            {
                o.Authority = $"{_apiServerRootUrl}/{_apiKeyValidationPath}";
                o.Method = HttpMethod.Get;
            };

            _postOptions = o =>
            {
                o.Authority = $"{_apiServerRootUrl}/{_apiKeyValidationPath}";
                o.Method = HttpMethod.Post;
            };
        }

        [Fact]
        public void should_return_401_using_api_key()
        {
            var port = RandomPortFinder.GetRandomPort();

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                {
                    new FileRoute
                    {
                        DownstreamPathTemplate = _downstreamServicePath,
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = _downstreamServiceHost,
                                Port = port
                            }
                        },
                        DownstreamScheme = _downstreamServiceScheme,
                        UpstreamPathTemplate = "/",
                        UpstreamHttpMethod = new List<string>{"Post"},
                        AuthenticationOptions = new FileAuthenticationOptions
                        {
                            AuthenticationProviderKey = "TestApiKey"
                        }
                    }
                }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"{_downstreamServiceUrl}{port}", 201, string.Empty))
                .And(x => x.GivenThereIsAMockKeyValidationApiServerOn(_apiServerRootUrl, _apiKeyValidationPath, new string[] { "testing" }, new string[] { }))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning(_getOptions, "TestApiKey"))
                .And(x => _steps.GivenThePostHasContent("postContent"))
                .When(x => _steps.WhenIPostUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.Unauthorized))
                .BDDfy();
        }

        [Fact]
        public void should_return_201_using_api_key()
        {
            var port = RandomPortFinder.GetRandomPort();

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                {
                    new FileRoute
                    {
                        DownstreamPathTemplate = _downstreamServicePath,
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = _downstreamServiceHost,
                                Port = port
                            }
                        },
                        DownstreamScheme = _downstreamServiceScheme,
                        UpstreamPathTemplate = "/",
                        UpstreamHttpMethod = new List<string>{"Post"},
                        AuthenticationOptions = new FileAuthenticationOptions
                        {
                            AuthenticationProviderKey = "TestApiKey",
                        }
                    }
                }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"{_downstreamServiceUrl}{port}", 201, string.Empty))
                .And(x => x.GivenThereIsAMockKeyValidationApiServerOn(_apiServerRootUrl, _apiKeyValidationPath, new string[] { "testing" }, new string[] { }))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning(_getOptions, "TestApiKey"))
                .And(x => _steps.GivenThePostHasContent("postContent"))
                .When(x => _steps.WhenIPostUrlOnTheApiGateway("/?key=testing"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.Created))
                .BDDfy();
        }

        [Fact]
        public void should_return_401_using_post()
        {
            var port = RandomPortFinder.GetRandomPort();

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                {
                    new FileRoute
                    {
                        DownstreamPathTemplate = _downstreamServicePath,
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = _downstreamServiceHost,
                                Port = port
                            }
                        },
                        DownstreamScheme = _downstreamServiceScheme,
                        UpstreamPathTemplate = "/",
                        UpstreamHttpMethod = new List<string>{"Post"},
                        AuthenticationOptions = new FileAuthenticationOptions
                        {
                            AuthenticationProviderKey = "TestApiKey"
                        }
                    }
                }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"{_downstreamServiceUrl}{port}", 201, string.Empty))
                .And(x => x.GivenThereIsAMockKeyValidationApiServerOn(_apiServerRootUrl, _apiKeyValidationPath, new string[] { "testing" }, new string[] { }))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning(_postOptions, "TestApiKey"))
                .And(x => _steps.GivenThePostHasContent("postContent"))
                .When(x => _steps.WhenIPostUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.Unauthorized))
                .BDDfy();
        }

        [Fact]
        public void should_return_201_using_api_key_with_post()
        {
            var port = RandomPortFinder.GetRandomPort();

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                {
                    new FileRoute
                    {
                        DownstreamPathTemplate = _downstreamServicePath,
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = _downstreamServiceHost,
                                Port = port
                            }
                        },
                        DownstreamScheme = _downstreamServiceScheme,
                        UpstreamPathTemplate = "/",
                        UpstreamHttpMethod = new List<string>{"Post"},
                        AuthenticationOptions = new FileAuthenticationOptions
                        {
                            AuthenticationProviderKey = "TestApiKey"
                        }
                    }
                }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"{_downstreamServiceUrl}{port}", 201, string.Empty))
                .And(x => x.GivenThereIsAMockKeyValidationApiServerOn(_apiServerRootUrl, _apiKeyValidationPath, new string[] { "testing" }, new string[] { }))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning(_postOptions, "TestApiKey"))
                .And(x => _steps.GivenThePostHasContent("postContent"))
                .When(x => _steps.WhenIPostUrlOnTheApiGateway("/?key=testing"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.Created))
                .BDDfy();
        }

        [Fact]
        public void should_return_403_when_user_has_incorrect_role()
        {
            var port = RandomPortFinder.GetRandomPort();

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                {
                    new FileRoute
                    {
                        DownstreamPathTemplate = _downstreamServicePath,
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = _downstreamServiceHost,
                                Port = port
                            }
                        },
                        DownstreamScheme = _downstreamServiceScheme,
                        UpstreamPathTemplate = "/",
                        UpstreamHttpMethod = new List<string>{"Post"},
                        RouteClaimsRequirement = new Dictionary<string, string>
                        {
                            { "Role", "testing" }
                        },
                        AuthenticationOptions = new FileAuthenticationOptions
                        {
                            AuthenticationProviderKey = "TestApiKey",
                        }
                    }
                }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"{_downstreamServiceUrl}{port}", 201, string.Empty))
                .And(x => x.GivenThereIsAMockKeyValidationApiServerOn(_apiServerRootUrl, _apiKeyValidationPath, new string[] { "testing" }, new string[] { }))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning(_getOptions, "TestApiKey"))
                .And(x => _steps.GivenThePostHasContent("postContent"))
                .When(x => _steps.WhenIPostUrlOnTheApiGateway("/?key=testing"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.Forbidden))
                .BDDfy();
        }

        [Fact]
        public void should_return_201_when_user_has_correct_role()
        {
            var port = RandomPortFinder.GetRandomPort();

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                {
                    new FileRoute
                    {
                        DownstreamPathTemplate = _downstreamServicePath,
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = _downstreamServiceHost,
                                Port = port
                            }
                        },
                        DownstreamScheme = _downstreamServiceScheme,
                        UpstreamPathTemplate = "/",
                        UpstreamHttpMethod = new List<string>{"Post"},
                        RouteClaimsRequirement = new Dictionary<string, string>
                        {
                            { "Role", "testing" }
                        },
                        AuthenticationOptions = new FileAuthenticationOptions
                        {
                            AuthenticationProviderKey = "TestApiKey",
                        }
                    }
                }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"{_downstreamServiceUrl}{port}", 201, string.Empty))
                .And(x => x.GivenThereIsAMockKeyValidationApiServerOn(_apiServerRootUrl, _apiKeyValidationPath, new string[] { "testing" }, new string[] { "testing" }))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning(_getOptions, "TestApiKey"))
                .And(x => _steps.GivenThePostHasContent("postContent"))
                .When(x => _steps.WhenIPostUrlOnTheApiGateway("/?key=testing"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.Created))
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

        private void GivenThereIsAMockKeyValidationApiServerOn(string url, string validationPath, string[] acceptedKeys, string[] roles)
        {
            var testBody = new
            {
                Owner = "testuser",
                Roles = roles
            };

            IWebHost test = new WebHostBuilder()
                .UseUrls(url)
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseUrls(url)
                .ConfigureServices(services =>
                {
                    services.AddRouting();
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapGet("/", async context =>
                        {
                            await context.Response.WriteAsync("OK");
                        });

                        endpoints.MapGet($"/{validationPath}", async context =>
                        {
                            if (!context.Request.Query.TryGetValue("key", out var key))
                            {
                                context.Response.StatusCode = 401;
                                await context.Response.WriteAsync("");
                            }

                            if (acceptedKeys.Any(x => x == key))
                            {
                                context.Response.StatusCode = 200;
                                await context.Response.WriteAsync(JsonConvert.SerializeObject(testBody));
                            }
                            else
                            {
                                context.Response.StatusCode = 401;
                                await context.Response.WriteAsync("");
                            }
                        });

                        endpoints.MapPost($"/{validationPath}", async context =>
                        {
                            StreamReader r = new StreamReader(context.Request.Body);
                            var streamBody = await r.ReadToEndAsync();

                            var body = JsonConvert.DeserializeObject<PostBody>(streamBody);
                            var key = body.key;

                            if (acceptedKeys.Any(x => x == key))
                            {
                                context.Response.StatusCode = 200;
                                await context.Response.WriteAsync(JsonConvert.SerializeObject(testBody));
                            }
                            else
                            {
                                context.Response.StatusCode = 401;
                                await context.Response.WriteAsync("");
                            }
                        });
                    });
                })
                .Build();

            test.Start();
            VerifyServerStarted(_apiServerRootUrl);
        }

        public void VerifyServerStarted(string url)
        {
            using (var httpClient = new HttpClient())
            {
                var response = httpClient.GetAsync(url).GetAwaiter().GetResult();
                var content = response.Content.ReadAsStringAsync().GetAwaiter();
                response.EnsureSuccessStatusCode();
            }
        }

        public void Dispose()
        {
            _serviceHandler.Dispose();
            _steps.Dispose();
        }
    }

    public class PostBody
    {
        public string key { get; set; }
    }
}


