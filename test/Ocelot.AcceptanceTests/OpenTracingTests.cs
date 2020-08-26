namespace Ocelot.AcceptanceTests
{
    using Butterfly.Client.AspNetCore;
    using Ocelot.Configuration.File;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using OpenTracing;
    using OpenTracing.Propagation;
    using OpenTracing.Tag;
    using Rafty.Infrastructure;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using TestStack.BDDfy;
    using Xunit;
    using Xunit.Abstractions;

    public class OpenTracingTests : IDisposable
    {
        private IWebHost _serviceOneBuilder;
        private IWebHost _serviceTwoBuilder;
        private IWebHost _fakeOpenTracing;
        private readonly Steps _steps;
        private string _downstreamPathOne;
        private string _downstreamPathTwo;
        private readonly ITestOutputHelper _output;

        public OpenTracingTests(ITestOutputHelper output)
        {
            _output = output;
            _steps = new Steps();
        }

        [Fact]
        public void should_forward_tracing_information_from_ocelot_and_downstream_services()
        {
            int port1 = RandomPortFinder.GetRandomPort();
            int port2 = RandomPortFinder.GetRandomPort();
            var configuration = new FileConfiguration()
            {
                Routes = new List<FileRoute>()
                    {
                        new FileRoute()
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
                        new FileRoute()
                        {
                            DownstreamPathTemplate = "/api/values",
                            DownstreamScheme = "http",
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort()
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
            
            var tracingPort = RandomPortFinder.GetRandomPort();
            var tracingUrl = $"http://localhost:{tracingPort}";

            var fakeTracer = new FakeTracer();

            this.Given(_ => GivenFakeOpenTracing(tracingUrl))
                .And(_ => GivenServiceOneIsRunning($"http://localhost:{port1}", "/api/values", 200, "Hello from Laura", tracingUrl))
                .And(_ => GivenServiceTwoIsRunning($"http://localhost:{port2}", "/api/values", 200, "Hello from Tom", tracingUrl))
                .And(_ => _steps.GivenThereIsAConfiguration(configuration))
                .And(_ => _steps.GivenOcelotIsRunningUsingOpenTracing(fakeTracer))
                .When(_ => _steps.WhenIGetUrlOnTheApiGateway("/api001/values"))
                .Then(_ => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(_ => _steps.ThenTheResponseBodyShouldBe("Hello from Laura"))
                .When(_ => _steps.WhenIGetUrlOnTheApiGateway("/api002/values"))
                .Then(_ => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(_ => _steps.ThenTheResponseBodyShouldBe("Hello from Tom"))
                .And(_ => ThenTheTracerIsCalled(fakeTracer))
                .BDDfy();
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

            var fakeTracer = new FakeTracer();

            this.Given(x => GivenFakeOpenTracing(butterflyUrl))
                .And(x => GivenServiceOneIsRunning($"http://localhost:{port}", "/api/values", 200, "Hello from Laura", butterflyUrl))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunningUsingOpenTracing(fakeTracer))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/api001/values"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from Laura"))
                .And(x => _steps.ThenTheTraceHeaderIsSet("Trace-Id"))
                .And(x => _steps.ThenTheResponseHeaderIs("Tom", "Laura"))
                .BDDfy();
        }

        private void ThenTheTracerIsCalled(FakeTracer fakeTracer)
        {
            var commandOnAllStateMachines = Wait.WaitFor(10000).Until(() => fakeTracer.BuildSpanCalled >= 2);

            _output.WriteLine($"fakeTracer.BuildSpanCalled is {fakeTracer.BuildSpanCalled}");

            commandOnAllStateMachines.ShouldBeTrue();
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

        private void GivenFakeOpenTracing(string baseUrl)
        {
            _fakeOpenTracing = new WebHostBuilder()
                .UseUrls(baseUrl)
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .Configure(app =>
                {
                    app.Run(async context =>
                    {
                        await context.Response.WriteAsync("OK...");
                    });
                })
                .Build();

            _fakeOpenTracing.Start();
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
            _fakeOpenTracing?.Dispose();
            _steps.Dispose();
        }
    }

    internal class FakeTracer : ITracer
    {
        public IScopeManager ScopeManager => throw new NotImplementedException();

        public ISpan ActiveSpan => throw new NotImplementedException();

        public ISpanBuilder BuildSpan(string operationName)
        {
            this.BuildSpanCalled++;

            return new FakeSpanBuilder();
        }

        public int BuildSpanCalled { get; set; }

        public ISpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier)
        {
            this.ExtractCalled++;

            return null;
        }

        public int ExtractCalled { get; set; }

        public void Inject<TCarrier>(ISpanContext spanContext, IFormat<TCarrier> format, TCarrier carrier)
        {
            this.InjectCalled++;
        }

        public int InjectCalled { get; set; }
    }

    internal class FakeSpanBuilder : ISpanBuilder
    {
        public ISpanBuilder AddReference(string referenceType, ISpanContext referencedContext)
        {
            throw new NotImplementedException();
        }

        public ISpanBuilder AsChildOf(ISpanContext parent)
        {
            throw new NotImplementedException();
        }

        public ISpanBuilder AsChildOf(ISpan parent)
        {
            throw new NotImplementedException();
        }

        public ISpanBuilder IgnoreActiveSpan()
        {
            throw new NotImplementedException();
        }

        public ISpan Start()
        {
            throw new NotImplementedException();
        }

        public IScope StartActive()
        {
            throw new NotImplementedException();
        }

        public IScope StartActive(bool finishSpanOnDispose)
        {
            return new FakeScope(finishSpanOnDispose);
        }

        public ISpanBuilder WithStartTimestamp(DateTimeOffset timestamp)
        {
            throw new NotImplementedException();
        }

        public ISpanBuilder WithTag(string key, string value)
        {
            throw new NotImplementedException();
        }

        public ISpanBuilder WithTag(string key, bool value)
        {
            throw new NotImplementedException();
        }

        public ISpanBuilder WithTag(string key, int value)
        {
            throw new NotImplementedException();
        }

        public ISpanBuilder WithTag(string key, double value)
        {
            throw new NotImplementedException();
        }

        public ISpanBuilder WithTag(BooleanTag tag, bool value)
        {
            throw new NotImplementedException();
        }

        public ISpanBuilder WithTag(IntOrStringTag tag, string value)
        {
            throw new NotImplementedException();
        }

        public ISpanBuilder WithTag(IntTag tag, int value)
        {
            throw new NotImplementedException();
        }

        public ISpanBuilder WithTag(StringTag tag, string value)
        {
            throw new NotImplementedException();
        }
    }

    internal class FakeScope : IScope
    {
        private readonly bool finishSpanOnDispose;

        public FakeScope(bool finishSpanOnDispose)
        {
            this.finishSpanOnDispose = finishSpanOnDispose;
        }

        public ISpan Span { get; } = new FakeSpan();

        public void Dispose()
        {
            if (this.finishSpanOnDispose)
            {
                this.Span.Finish();
            }
        }
    }

    internal class FakeSpan : ISpan
    {
        public ISpanContext Context => new FakeSpanContext();

        public void Finish()
        {
        }

        public void Finish(DateTimeOffset finishTimestamp)
        {
            throw new NotImplementedException();
        }

        public string GetBaggageItem(string key)
        {
            throw new NotImplementedException();
        }

        public ISpan Log(IEnumerable<KeyValuePair<string, object>> fields)
        {
            return this;
        }

        public ISpan Log(DateTimeOffset timestamp, IEnumerable<KeyValuePair<string, object>> fields)
        {
            throw new NotImplementedException();
        }

        public ISpan Log(string @event)
        {
            throw new NotImplementedException();
        }

        public ISpan Log(DateTimeOffset timestamp, string @event)
        {
            throw new NotImplementedException();
        }

        public ISpan SetBaggageItem(string key, string value)
        {
            throw new NotImplementedException();
        }

        public ISpan SetOperationName(string operationName)
        {
            throw new NotImplementedException();
        }

        public ISpan SetTag(string key, string value)
        {
            return this;
        }

        public ISpan SetTag(string key, bool value)
        {
            return this;
        }

        public ISpan SetTag(string key, int value)
        {
            return this;
        }

        public ISpan SetTag(string key, double value)
        {
            return this;
        }

        public ISpan SetTag(BooleanTag tag, bool value)
        {
            return this;
        }

        public ISpan SetTag(IntOrStringTag tag, string value)
        {
            return this;
        }

        public ISpan SetTag(IntTag tag, int value)
        {
            return this;
        }

        public ISpan SetTag(StringTag tag, string value)
        {
            return this;
        }
    }

    internal class FakeSpanContext : ISpanContext
    {
        public static string FakeTraceId = "FakeTraceId";

        public static string FakeSpanId = "FakeSpanId";

        public string TraceId => FakeTraceId;

        public string SpanId => FakeSpanId;

        public IEnumerable<KeyValuePair<string, string>> GetBaggageItems()
        {
            throw new NotImplementedException();
        }
    }
}
