namespace Ocelot.AcceptanceTests
{
    using Butterfly.Client.AspNetCore;
    using Configuration.File;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Rafty.Infrastructure;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using TestStack.BDDfy;
    using Xunit;
    using Xunit.Abstractions;

    public class ButterflyTracingTests : IDisposable
    {
        private IWebHost _serviceOneBuilder;
        private IWebHost _serviceTwoBuilder;
        private IWebHost _fakeButterfly;
        private readonly Steps _steps;
        private string _downstreamPathOne;
        private string _downstreamPathTwo;
        private int _butterflyCalled;
        private readonly ITestOutputHelper _output;

        public ButterflyTracingTests(ITestOutputHelper output)
        {
            _output = output;
            _steps = new Steps();
        }

        [Fact]
        public void should_forward_tracing_information_from_ocelot_and_downstream_services()
        {
            int port1 = RandomPortFinder.GetRandomPort();
            int port2 = RandomPortFinder.GetRandomPort();
            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                    {
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/api/values",
                            DownstreamScheme = "http",
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = port1,
                                }
                            },
                            UpstreamPathTemplate = "/api001/values",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            HttpHandlerOptions = new FileHttpHandlerOptions
                            {
                                UseTracing = true
                            }
                        },
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/api/values",
                            DownstreamScheme = "http",
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = port2,
                                }
                            },
                            UpstreamPathTemplate = "/api002/values",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            HttpHandlerOptions = new FileHttpHandlerOptions
                            {
                                UseTracing = true
                            }
                        }
                    }
            };
            
            var butterflyPort = RandomPortFinder.GetRandomPort();
            var butterflyUrl = $"http://localhost:{butterflyPort}";

            this.Given(x => GivenFakeButterfly(butterflyUrl))
                .And(x => GivenServiceOneIsRunning($"http://localhost:{port1}", "/api/values", 200, "Hello from Laura", butterflyUrl))
                .And(x => GivenServiceTwoIsRunning($"http://localhost:{port2}", "/api/values", 200, "Hello from Tom", butterflyUrl))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunningUsingButterfly(butterflyUrl))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/api001/values"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from Laura"))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/api002/values"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from Tom"))
                .BDDfy();

            var commandOnAllStateMachines = Wait.WaitFor(10000).Until(() => _butterflyCalled >= 4);

            _output.WriteLine($"_butterflyCalled is {_butterflyCalled}");

            commandOnAllStateMachines.ShouldBeTrue();
        }

        [Fact]
        public void should_return_tracing_header()
        {
            int port = RandomPortFinder.GetRandomPort();
            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                    {
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/api/values",
                            DownstreamScheme = "http",
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = port,
                                }
                            },
                            UpstreamPathTemplate = "/api001/values",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            HttpHandlerOptions = new FileHttpHandlerOptions
                            {
                                UseTracing = true
                            },
                            DownstreamHeaderTransform = new Dictionary<string, string>()
                            {
                                {"Trace-Id", "{TraceId}"},
                                {"Tom", "Laura"}
                            }
                        }
                    }
            };

            var butterflyPort = RandomPortFinder.GetRandomPort();
            var butterflyUrl = $"http://localhost:{butterflyPort}";

            this.Given(x => GivenFakeButterfly(butterflyUrl))
                .And(x => GivenServiceOneIsRunning($"http://localhost:{port}", "/api/values", 200, "Hello from Laura", butterflyUrl))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunningUsingButterfly(butterflyUrl))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/api001/values"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from Laura"))
                .And(x => _steps.ThenTheTraceHeaderIsSet("Trace-Id"))
                .And(x => _steps.ThenTheResponseHeaderIs("Tom", "Laura"))
                .BDDfy();
        }

        private void GivenServiceOneIsRunning(string baseUrl, string basePath, int statusCode, string responseBody, string butterflyUrl)
        {
            _serviceOneBuilder = new WebHostBuilder()
                .UseUrls(baseUrl)
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .ConfigureServices(services =>
                {
                    services.AddButterfly(option =>
                    {
                        option.CollectorUrl = butterflyUrl;
                        option.Service = "Service One";
                        option.IgnoredRoutesRegexPatterns = new string[0];
                    });
                })
                .Configure(app =>
                {
                    app.UsePathBase(basePath);
                    app.Run(async context =>
                    {
                        _downstreamPathOne = !string.IsNullOrEmpty(context.Request.PathBase.Value) ? context.Request.PathBase.Value : context.Request.Path.Value;

                        if (_downstreamPathOne != basePath)
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
                })
                .Build();

            _serviceOneBuilder.Start();
        }

        private void GivenFakeButterfly(string baseUrl)
        {
            _fakeButterfly = new WebHostBuilder()
                .UseUrls(baseUrl)
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .Configure(app =>
                {
                    app.Run(async context =>
                    {
                        _butterflyCalled++;
                        await context.Response.WriteAsync("OK...");
                    });
                })
                .Build();

            _fakeButterfly.Start();
        }

        private void GivenServiceTwoIsRunning(string baseUrl, string basePath, int statusCode, string responseBody, string butterflyUrl)
        {
            _serviceTwoBuilder = new WebHostBuilder()
                .UseUrls(baseUrl)
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .ConfigureServices(services =>
                {
                    services.AddButterfly(option =>
                    {
                        option.CollectorUrl = butterflyUrl;
                        option.Service = "Service Two";
                        option.IgnoredRoutesRegexPatterns = new string[0];
                    });
                })
                .Configure(app =>
                {
                    app.UsePathBase(basePath);
                    app.Run(async context =>
                    {
                        _downstreamPathTwo = !string.IsNullOrEmpty(context.Request.PathBase.Value) ? context.Request.PathBase.Value : context.Request.Path.Value;

                        if (_downstreamPathTwo != basePath)
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
                })
                .Build();

            _serviceTwoBuilder.Start();
        }

        public void Dispose()
        {
            _serviceOneBuilder?.Dispose();
            _serviceTwoBuilder?.Dispose();
            _fakeButterfly?.Dispose();
            _steps.Dispose();
        }
    }
}
