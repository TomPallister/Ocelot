using Microsoft.AspNetCore.Http;
using Ocelot.Configuration.File;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.AcceptanceTests
{
    public class RoutingBasedOnHeadersTests : IDisposable
    {
        private readonly Steps _steps;
        private string _downstreamPath;
        private readonly ServiceHandler _serviceHandler;

        public RoutingBasedOnHeadersTests()
        {
            _serviceHandler = new ServiceHandler();
            _steps = new Steps();
        }

        [Fact]
        public void should_match_one_header_value()
        {
            var port = RandomPortFinder.GetRandomPort();
            var headerName = "country_code";
            var headerValue = "PL";

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                    {
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/",
                            DownstreamScheme = "http",
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = port,
                                },
                            },
                            UpstreamPathTemplate = "/",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            UpstreamHeaders = new Dictionary<string, string>() 
                            {
                                [headerName] = headerValue,
                            },
                        },
                    },
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/", 200, "Hello from Laura"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .And(x => _steps.GivenIAddAHeader(headerName, headerValue))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from Laura"))
                .BDDfy();
        }
        
        [Fact]
        public void should_match_one_header_value_when_more_headers()
        {
            var port = RandomPortFinder.GetRandomPort();
            var headerName = "country_code";
            var headerValue = "PL";

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                    {
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/",
                            DownstreamScheme = "http",
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = port,
                                },
                            },
                            UpstreamPathTemplate = "/",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            UpstreamHeaders = new Dictionary<string, string>()
                            {
                                [headerName] = headerValue,
                            },
                        },
                    },
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/", 200, "Hello from Laura"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .And(x => _steps.GivenIAddAHeader("other", "otherValue"))
                .And(x => _steps.GivenIAddAHeader(headerName, headerValue))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from Laura"))
                .BDDfy();
        }

        [Fact]
        public void should_match_two_header_values_when_more_headers()
        {
            var port = RandomPortFinder.GetRandomPort();
            var headerName1 = "country_code";
            var headerValue1 = "PL";
            var headerName2 = "region";
            var headerValue2 = "MAZ";

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                    {
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/",
                            DownstreamScheme = "http",
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = port,
                                },
                            },
                            UpstreamPathTemplate = "/",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            UpstreamHeaders = new Dictionary<string, string>()
                            {
                                [headerName1] = headerValue1,
                                [headerName2] = headerValue2,
                            },
                        },
                    },
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/", 200, "Hello from Laura"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .And(x => _steps.GivenIAddAHeader(headerName1, headerValue1))
                .And(x => _steps.GivenIAddAHeader("other", "otherValue"))
                .And(x => _steps.GivenIAddAHeader(headerName2, headerValue2))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from Laura"))
                .BDDfy();
        }

        [Fact]
        public void should_not_match_one_header_value()
        {
            var port = RandomPortFinder.GetRandomPort();
            var headerName = "country_code";
            var headerValue = "PL";
            var anotherHeaderValue = "UK";

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                    {
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/",
                            DownstreamScheme = "http",
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = port,
                                },
                            },
                            UpstreamPathTemplate = "/",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            UpstreamHeaders = new Dictionary<string, string>()
                            {
                                [headerName] = headerValue,
                            },
                        },
                    },
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/", 200, "Hello from Laura"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .And(x => _steps.GivenIAddAHeader(headerName, anotherHeaderValue))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.NotFound))
                .BDDfy();
        }

        [Fact]
        public void should_not_match_one_header_value_when_no_headers()
        {
            var port = RandomPortFinder.GetRandomPort();
            var headerName = "country_code";
            var headerValue = "PL";

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                    {
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/",
                            DownstreamScheme = "http",
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = port,
                                },
                            },
                            UpstreamPathTemplate = "/",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            UpstreamHeaders = new Dictionary<string, string>()
                            {
                                [headerName] = headerValue,
                            },
                        },
                    },
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/", 200, "Hello from Laura"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.NotFound))
                .BDDfy();
        }

        [Fact]
        public void should_not_match_two_header_values_when_one_different()
        {
            var port = RandomPortFinder.GetRandomPort();
            var headerName1 = "country_code";
            var headerValue1 = "PL";
            var headerName2 = "region";
            var headerValue2 = "MAZ";

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                    {
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/",
                            DownstreamScheme = "http",
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = port,
                                },
                            },
                            UpstreamPathTemplate = "/",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            UpstreamHeaders = new Dictionary<string, string>()
                            {
                                [headerName1] = headerValue1,
                                [headerName2] = headerValue2,
                            },
                        },
                    },
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/", 200, "Hello from Laura"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .And(x => _steps.GivenIAddAHeader(headerName1, headerValue1))
                .And(x => _steps.GivenIAddAHeader("other", "otherValue"))
                .And(x => _steps.GivenIAddAHeader(headerName2, "anothervalue"))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.NotFound))
                .BDDfy();
        }

        [Fact]
        public void should_not_match_two_header_values_when_one_not_existing()
        {
            var port = RandomPortFinder.GetRandomPort();
            var headerName1 = "country_code";
            var headerValue1 = "PL";
            var headerName2 = "region";
            var headerValue2 = "MAZ";

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                    {
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/",
                            DownstreamScheme = "http",
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = port,
                                },
                            },
                            UpstreamPathTemplate = "/",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            UpstreamHeaders = new Dictionary<string, string>()
                            {
                                [headerName1] = headerValue1,
                                [headerName2] = headerValue2,
                            },
                        },
                    },
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/", 200, "Hello from Laura"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .And(x => _steps.GivenIAddAHeader(headerName1, headerValue1))
                .And(x => _steps.GivenIAddAHeader("other", "otherValue"))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.NotFound))
                .BDDfy();
        }

        [Fact]
        public void should_aggregated_route_match_header_value()
        {
            var port1 = RandomPortFinder.GetRandomPort();
            var port2 = RandomPortFinder.GetRandomPort();
            var headerName = "country_code";
            var headerValue = "PL";

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                    {
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/a",
                            DownstreamScheme = "http",
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = port1,
                                },
                            },
                            UpstreamPathTemplate = "/a",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            Key = "Laura",
                        },
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/b",
                            DownstreamScheme = "http",
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = port2,
                                },
                            },
                            UpstreamPathTemplate = "/b",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            Key = "Tom",
                        },
                    },
                Aggregates = new List<FileAggregateRoute>()
                {
                    new FileAggregateRoute
                        {
                            UpstreamPathTemplate = "/",
                            UpstreamHost = "localhost",
                            RouteKeys = new List<string>
                            {
                                "Laura",
                                "Tom",
                            },
                            UpstreamHeaders = new Dictionary<string, string>()
                            {
                                [headerName] = headerValue,
                            },
                        },
                },
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port1}", "/a", 200, "Hello from Laura"))
                .And(x => GivenThereIsAServiceRunningOn($"http://localhost:{port2}", "/b", 200, "Hello from Tom"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .And(x => _steps.GivenIAddAHeader(headerName, headerValue))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .BDDfy();
        }

        [Fact]
        public void should_aggregated_route_not_match_header_value()
        {
            var port1 = RandomPortFinder.GetRandomPort();
            var port2 = RandomPortFinder.GetRandomPort();
            var headerName = "country_code";
            var headerValue = "PL";

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                    {
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/a",
                            DownstreamScheme = "http",
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = port1,
                                },
                            },
                            UpstreamPathTemplate = "/a",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            Key = "Laura",
                        },
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/b",
                            DownstreamScheme = "http",
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = port2,
                                },
                            },
                            UpstreamPathTemplate = "/b",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            Key = "Tom",
                        },
                    },
                Aggregates = new List<FileAggregateRoute>()
                {
                    new FileAggregateRoute
                        {
                            UpstreamPathTemplate = "/",
                            UpstreamHost = "localhost",
                            RouteKeys = new List<string>
                            {
                                "Laura",
                                "Tom",
                            },
                            UpstreamHeaders = new Dictionary<string, string>()
                            {
                                [headerName] = headerValue,
                            },
                        },
                },
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port1}", "/a", 200, "Hello from Laura"))
                .And(x => GivenThereIsAServiceRunningOn($"http://localhost:{port2}", "/b", 200, "Hello from Tom"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.NotFound))
                .BDDfy();
        }        

        private void GivenThereIsAServiceRunningOn(string baseUrl, string basePath, int statusCode, string responseBody)
        {
            _serviceHandler.GivenThereIsAServiceRunningOn(baseUrl, basePath, async context =>
            {
                _downstreamPath = !string.IsNullOrEmpty(context.Request.PathBase.Value) ? context.Request.PathBase.Value : context.Request.Path.Value;

                if (_downstreamPath != basePath)
                {
                    context.Response.StatusCode = statusCode;
                    await context.Response.WriteAsync("downstream path didnt match base path");
                }
                else
                {
                    context.Response.StatusCode = statusCode;
                    await context.Response.WriteAsync(responseBody);
                }
            });
        }

        internal void ThenTheDownstreamUrlPathShouldBe(string expectedDownstreamPath)
        {
            _downstreamPath.ShouldBe(expectedDownstreamPath);
        }

        public void Dispose()
        {
            _serviceHandler.Dispose();
            _steps.Dispose();
        }
    }
}
