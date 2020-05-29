namespace Ocelot.AcceptanceTests
{
    using Configuration.File;
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using TestStack.BDDfy;
    using Xunit;

    public class PollyQoSTests : IDisposable
    {
        private readonly Steps _steps;
        private int _requestCount;
        private readonly ServiceHandler _serviceHandler;

        public PollyQoSTests()
        {
            _serviceHandler = new ServiceHandler();
            _steps = new Steps();
        }

        [Fact]
        public void should_not_timeout()
        {
            var port = RandomPortFinder.GetRandomPort();

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
                        UpstreamHttpMethod = new List<string> { "Post" },
                        QoSOptions = new FileQoSOptions
                        {
                            TimeoutValue = 1000,
                            ExceptionsAllowedBeforeBreaking = 10
                        }
                    }
                }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", 200, string.Empty, 10))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunningWithPolly())
                .And(x => _steps.GivenThePostHasContent("postContent"))
                .When(x => _steps.WhenIPostUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .BDDfy();
        }

        [Fact]
        public void should_timeout()
        {
            var port = RandomPortFinder.GetRandomPort();

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
                        UpstreamHttpMethod = new List<string> { "Post" },
                        QoSOptions = new FileQoSOptions
                        {
                            TimeoutValue = 10,
                            ExceptionsAllowedBeforeBreaking = 10
                        }
                    }
                }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", 201, string.Empty, 1000))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunningWithPolly())
                .And(x => _steps.GivenThePostHasContent("postContent"))
                .When(x => _steps.WhenIPostUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.ServiceUnavailable))
                .BDDfy();
        }

        [Fact]
        public void should_open_circuit_breaker_then_close()
        {
            var port = RandomPortFinder.GetRandomPort();

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
                            }
                        },
                        UpstreamPathTemplate = "/",
                        UpstreamHttpMethod = new List<string> { "Get" },
                        QoSOptions = new FileQoSOptions
                        {
                            ExceptionsAllowedBeforeBreaking = 1,
                            TimeoutValue = 500,
                            DurationOfBreak = 1000
                        },
                    }
                }
            };

            this.Given(x => x.GivenThereIsAPossiblyBrokenServiceRunningOn($"http://localhost:{port}", "Hello from Laura"))
                .Given(x => _steps.GivenThereIsAConfiguration(configuration))
                .Given(x => _steps.GivenOcelotIsRunningWithPolly())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from Laura"))
                .Given(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Given(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.ServiceUnavailable))
                .Given(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Given(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.ServiceUnavailable))
                .Given(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Given(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.ServiceUnavailable))
                .Given(x => x.GivenIWaitMilliseconds(3000))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from Laura"))
                .BDDfy();
        }

        [Fact]
        public void open_circuit_should_not_effect_different_route()
        {
            var port1 = RandomPortFinder.GetRandomPort();
            var port2 = RandomPortFinder.GetRandomPort();

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
                                Port = port1,
                            }
                        },
                        UpstreamPathTemplate = "/",
                        UpstreamHttpMethod = new List<string> { "Get" },
                        QoSOptions = new FileQoSOptions
                        {
                            ExceptionsAllowedBeforeBreaking = 1,
                            TimeoutValue = 500,
                            DurationOfBreak = 1000
                        }
                    },
                    new FileRoute
                    {
                        DownstreamPathTemplate = "/",
                        DownstreamScheme = "http",
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "localhost",
                                Port = port2,
                            }
                        },
                        UpstreamPathTemplate = "/working",
                        UpstreamHttpMethod = new List<string> { "Get" },
                    }
                }
            };

            this.Given(x => x.GivenThereIsAPossiblyBrokenServiceRunningOn($"http://localhost:{port1}", "Hello from Laura"))
                .And(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port2}/", 200, "Hello from Tom", 0))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunningWithPolly())
                .And(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .And(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from Laura"))
                .And(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .And(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.ServiceUnavailable))
                .And(x => _steps.WhenIGetUrlOnTheApiGateway("/working"))
                .And(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from Tom"))
                .And(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .And(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.ServiceUnavailable))
                .And(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .And(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.ServiceUnavailable))
                .And(x => x.GivenIWaitMilliseconds(3000))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from Laura"))
                .BDDfy();
        }

        private void GivenIWaitMilliseconds(int ms)
        {
            Thread.Sleep(ms);
        }

        private void GivenThereIsAPossiblyBrokenServiceRunningOn(string url, string responseBody)
        {
            _serviceHandler.GivenThereIsAServiceRunningOn(url, async context =>
            {
                //circuit starts closed
                if (_requestCount == 0)
                {
                    _requestCount++;
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync(responseBody);
                    return;
                }

                //request one times out and polly throws exception, circuit opens
                if (_requestCount == 1)
                {
                    _requestCount++;
                    await Task.Delay(1000);
                    context.Response.StatusCode = 200;
                    return;
                }

                //after break closes we return 200 OK
                if (_requestCount == 2)
                {
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync(responseBody);
                }
            });
        }

        private void GivenThereIsAServiceRunningOn(string url, int statusCode, string responseBody, int timeout)
        {
            _serviceHandler.GivenThereIsAServiceRunningOn(url, async context =>
            {
                Thread.Sleep(timeout);
                context.Response.StatusCode = statusCode;
                await context.Response.WriteAsync(responseBody);
            });
        }

        public void Dispose()
        {
            _serviceHandler?.Dispose();
            _steps.Dispose();
        }
    }
}
