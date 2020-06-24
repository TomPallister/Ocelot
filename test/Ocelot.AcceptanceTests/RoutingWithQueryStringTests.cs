namespace Ocelot.AcceptanceTests
{
    using Microsoft.AspNetCore.Http;
    using Ocelot.Configuration.File;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using TestStack.BDDfy;
    using Xunit;

    public class RoutingWithQueryStringTests : IDisposable
    {
        private readonly Steps _steps;
        private readonly ServiceHandler _serviceHandler;

        public RoutingWithQueryStringTests()
        {
            _serviceHandler = new ServiceHandler();
            _steps = new Steps();
        }

        [Fact]
        public void should_return_response_200_with_query_string_template()
        {
            var subscriptionId = Guid.NewGuid().ToString();
            var unitId = Guid.NewGuid().ToString();
            var port = RandomPortFinder.GetRandomPort();

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                    {
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/api/subscriptions/{subscriptionId}/updates?unitId={unitId}",
                            DownstreamScheme = "http",
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = port,
                                }
                            },
                            UpstreamPathTemplate = "/api/units/{subscriptionId}/{unitId}/updates",
                            UpstreamHttpMethod = new List<string> { "Get" },
                        }
                    }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", $"/api/subscriptions/{subscriptionId}/updates", $"?unitId={unitId}", 200, "Hello from Laura"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway($"/api/units/{subscriptionId}/{unitId}/updates"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from Laura"))
                .BDDfy();
        }

        [Fact]
        public void should_return_response_200_with_odata_query_string()
        {
            var subscriptionId = Guid.NewGuid().ToString();
            var unitId = Guid.NewGuid().ToString();
            var port = RandomPortFinder.GetRandomPort();

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                    {
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/{everything}",
                            DownstreamScheme = "http",
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = port,
                                }
                            },
                            UpstreamPathTemplate = "/{everything}",
                            UpstreamHttpMethod = new List<string> { "Get" },
                        }
                    }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", $"/odata/customers", "?$filter=Name%20eq%20'Sam'", 200, "Hello from Laura"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway($"/odata/customers?$filter=Name eq 'Sam' "))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from Laura"))
                .BDDfy();
        }

        [Fact]
        public void should_return_response_200_with_query_string_upstream_template()
        {
            var subscriptionId = Guid.NewGuid().ToString();
            var unitId = Guid.NewGuid().ToString();
            var port = RandomPortFinder.GetRandomPort();

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                {
                    new FileRoute
                    {
                        DownstreamPathTemplate = "/api/units/{subscriptionId}/{unitId}/updates",
                        DownstreamScheme = "http",
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "localhost",
                                Port = port,
                            }
                        },
                        UpstreamPathTemplate = "/api/subscriptions/{subscriptionId}/updates?unitId={unitId}",
                        UpstreamHttpMethod = new List<string> { "Get" },
                    }
                }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", $"/api/units/{subscriptionId}/{unitId}/updates", "", 200, "Hello from Laura"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway($"/api/subscriptions/{subscriptionId}/updates?unitId={unitId}"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from Laura"))
                .BDDfy();
        }

        [Fact]
        public void should_return_response_404_with_query_string_upstream_template_no_query_string()
        {
            var subscriptionId = Guid.NewGuid().ToString();
            var unitId = Guid.NewGuid().ToString();
            var port = RandomPortFinder.GetRandomPort();

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                {
                    new FileRoute
                    {
                        DownstreamPathTemplate = "/api/units/{subscriptionId}/{unitId}/updates",
                        DownstreamScheme = "http",
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "localhost",
                                Port = port,
                            }
                        },
                        UpstreamPathTemplate = "/api/subscriptions/{subscriptionId}/updates?unitId={unitId}",
                        UpstreamHttpMethod = new List<string> { "Get" },
                    }
                }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", $"/api/units/{subscriptionId}/{unitId}/updates", "", 200, "Hello from Laura"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway($"/api/subscriptions/{subscriptionId}/updates"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.NotFound))
                .BDDfy();
        }

        [Fact]
        public void should_return_response_404_with_query_string_upstream_template_different_query_string()
        {
            var subscriptionId = Guid.NewGuid().ToString();
            var unitId = Guid.NewGuid().ToString();
            var port = RandomPortFinder.GetRandomPort();

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                {
                    new FileRoute
                    {
                        DownstreamPathTemplate = "/api/units/{subscriptionId}/{unitId}/updates",
                        DownstreamScheme = "http",
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "localhost",
                                Port = port,
                            }
                        },
                        UpstreamPathTemplate = "/api/subscriptions/{subscriptionId}/updates?unitId={unitId}",
                        UpstreamHttpMethod = new List<string> { "Get" },
                    }
                }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", $"/api/units/{subscriptionId}/{unitId}/updates", "", 200, "Hello from Laura"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway($"/api/subscriptions/{subscriptionId}/updates?test=1"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.NotFound))
                .BDDfy();
        }

        [Fact]
        public void should_return_response_200_with_query_string_upstream_template_multiple_params()
        {
            var subscriptionId = Guid.NewGuid().ToString();
            var unitId = Guid.NewGuid().ToString();
            var port = RandomPortFinder.GetRandomPort();

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                {
                    new FileRoute
                    {
                        DownstreamPathTemplate = "/api/units/{subscriptionId}/{unitId}/updates",
                        DownstreamScheme = "http",
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "localhost",
                                Port = port,
                            }
                        },
                        UpstreamPathTemplate = "/api/subscriptions/{subscriptionId}/updates?unitId={unitId}",
                        UpstreamHttpMethod = new List<string> { "Get" },
                    }
                }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", $"/api/units/{subscriptionId}/{unitId}/updates", "?productId=1", 200, "Hello from Laura"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway($"/api/subscriptions/{subscriptionId}/updates?unitId={unitId}&productId=1"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from Laura"))
                .BDDfy();
        }

        private void GivenThereIsAServiceRunningOn(string baseUrl, string basePath, string queryString, int statusCode, string responseBody)
        {
            _serviceHandler.GivenThereIsAServiceRunningOn(baseUrl, basePath, async context =>
            {
                if ((context.Request.PathBase.Value != basePath) || context.Request.QueryString.Value != queryString)
                {
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("downstream path didnt match base path");
                }
                else
                {
                    context.Response.StatusCode = statusCode;
                    await context.Response.WriteAsync(responseBody);
                }
            });
        }

        public void Dispose()
        {
            _serviceHandler?.Dispose();
            _steps.Dispose();
        }
    }
}
