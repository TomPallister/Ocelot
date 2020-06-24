namespace Ocelot.AcceptanceTests
{
    using Microsoft.AspNetCore.Http;
    using Ocelot.Configuration.File;
    using Ocelot.Middleware;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Threading.Tasks;
    using TestStack.BDDfy;
    using Xunit;

    public class CustomMiddlewareTests : IDisposable
    {
        private readonly string _configurationPath;
        private readonly Steps _steps;
        private int _counter;
        private readonly ServiceHandler _serviceHandler;

        public CustomMiddlewareTests()
        {
            _serviceHandler = new ServiceHandler();
            _counter = 0;
            _steps = new Steps();
            _configurationPath = "ocelot.json";
        }

        [Fact]
        public void should_call_pre_query_string_builder_middleware()
        {
            var configuration = new OcelotPipelineConfiguration
            {
                AuthorisationMiddleware = async (ctx, next) =>
                {
                    _counter++;
                    await next.Invoke();
                }
            };

            var port = RandomPortFinder.GetRandomPort();

            var fileConfiguration = new FileConfiguration
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
                        }
                    }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", 200, ""))
                .And(x => _steps.GivenThereIsAConfiguration(fileConfiguration, _configurationPath))
                .And(x => _steps.GivenOcelotIsRunning(configuration))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => x.ThenTheCounterIs(1))
                .BDDfy();
        }

        [Fact]
        public void should_call_authorisation_middleware()
        {
            var configuration = new OcelotPipelineConfiguration
            {
                AuthorisationMiddleware = async (ctx, next) =>
                {
                    _counter++;
                    await next.Invoke();
                }
            };

            var port = RandomPortFinder.GetRandomPort();

            var fileConfiguration = new FileConfiguration
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
                        }
                    }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", 200, ""))
                .And(x => _steps.GivenThereIsAConfiguration(fileConfiguration, _configurationPath))
                .And(x => _steps.GivenOcelotIsRunning(configuration))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => x.ThenTheCounterIs(1))
                .BDDfy();
        }

        [Fact]
        public void should_call_authentication_middleware()
        {
            var configuration = new OcelotPipelineConfiguration
            {
                AuthenticationMiddleware = async (ctx, next) =>
                {
                    _counter++;
                    await next.Invoke();
                }
            };

            var port = RandomPortFinder.GetRandomPort();

            var fileConfiguration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                    {
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/41879/",
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
                        }
                    }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", 200, ""))
                .And(x => _steps.GivenThereIsAConfiguration(fileConfiguration, _configurationPath))
                .And(x => _steps.GivenOcelotIsRunning(configuration))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => x.ThenTheCounterIs(1))
                .BDDfy();
        }

        [Fact]
        public void should_call_pre_error_middleware()
        {
            var configuration = new OcelotPipelineConfiguration
            {
                PreErrorResponderMiddleware = async (ctx, next) =>
                {
                    _counter++;
                    await next.Invoke();
                }
            };

            var port = RandomPortFinder.GetRandomPort();

            var fileConfiguration = new FileConfiguration
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
                        }
                    }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", 200, ""))
                .And(x => _steps.GivenThereIsAConfiguration(fileConfiguration, _configurationPath))
                .And(x => _steps.GivenOcelotIsRunning(configuration))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => x.ThenTheCounterIs(1))
                .BDDfy();
        }

        [Fact]
        public void should_call_pre_authorisation_middleware()
        {
            var configuration = new OcelotPipelineConfiguration
            {
                PreAuthorisationMiddleware = async (ctx, next) =>
                {
                    _counter++;
                    await next.Invoke();
                }
            };

            var port = RandomPortFinder.GetRandomPort();

            var fileConfiguration = new FileConfiguration
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
                        }
                    }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", 200, ""))
                .And(x => _steps.GivenThereIsAConfiguration(fileConfiguration, _configurationPath))
                .And(x => _steps.GivenOcelotIsRunning(configuration))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => x.ThenTheCounterIs(1))
                .BDDfy();
        }

        [Fact]
        public void should_call_pre_http_authentication_middleware()
        {
            var configuration = new OcelotPipelineConfiguration
            {
                PreAuthenticationMiddleware = async (ctx, next) =>
                {
                    _counter++;
                    await next.Invoke();
                }
            };

            var port = RandomPortFinder.GetRandomPort();

            var fileConfiguration = new FileConfiguration
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
                        }
                    }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", 200, ""))
                .And(x => _steps.GivenThereIsAConfiguration(fileConfiguration, _configurationPath))
                .And(x => _steps.GivenOcelotIsRunning(configuration))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => x.ThenTheCounterIs(1))
                .BDDfy();
        }

        [Fact(Skip = "This is just an example to show how you could hook into Ocelot pipeline with your own middleware. At the moment you must use Response.OnCompleted callback and cannot change the response :( I will see if this can be changed one day!")]
        public void should_fix_issue_237()
        {
            Func<object, Task> callback = state =>
            {
                var httpContext = (HttpContext)state;

                if (httpContext.Response.StatusCode > 400)
                {
                    Debug.WriteLine("COUNT CALLED");
                    Console.WriteLine("COUNT CALLED");
                }

                return Task.CompletedTask;
            };

            var port = RandomPortFinder.GetRandomPort();

            var fileConfiguration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                    {
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/west",
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
                        }
                    }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", 200, "/test"))
                .And(x => _steps.GivenThereIsAConfiguration(fileConfiguration, _configurationPath))
                .And(x => _steps.GivenOcelotIsRunningWithMiddleareBeforePipeline<FakeMiddleware>(callback))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.NotFound))
                .BDDfy();
        }

        private void ThenTheCounterIs(int expected)
        {
            _counter.ShouldBe(expected);
        }

        private void GivenThereIsAServiceRunningOn(string url, int statusCode, string basePath)
        {
            _serviceHandler.GivenThereIsAServiceRunningOn(url, context =>
            {
                if (string.IsNullOrEmpty(basePath))
                {
                    context.Response.StatusCode = statusCode;
                }
                else if (context.Request.Path.Value != basePath)
                {
                    context.Response.StatusCode = 404;
                }

                return Task.CompletedTask;
            });
        }

        public void Dispose()
        {
            _serviceHandler?.Dispose();
            _steps.Dispose();
        }

        public class FakeMiddleware
        {
            private readonly RequestDelegate _next;
            private readonly Func<object, Task> _callback;

            public FakeMiddleware(RequestDelegate next, Func<object, Task> callback)
            {
                _next = next;
                _callback = callback;
            }

            public async Task Invoke(HttpContext context)
            {
                await _next(context);

                context.Response.OnCompleted(_callback, context);
            }
        }
    }
}
