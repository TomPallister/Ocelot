namespace Ocelot.AcceptanceTests
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Ocelot.Configuration.File;
    using System;
    using System.Collections.Generic;
    using TestStack.BDDfy;
    using Xunit;

    public class ReasonPhraseTests : IDisposable
    {
        private readonly Steps _steps;
        private string _contentType;
        private long? _contentLength;
        private bool _contentTypeHeaderExists;
        private readonly ServiceHandler _serviceHandler;

        public ReasonPhraseTests()
        {
            _serviceHandler = new ServiceHandler();
            _steps = new Steps();
        }

        [Fact]
        public void should_return_reason_phrase()
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
                        }
                    }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/", "some reason"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .And(_ => _steps.ThenTheReasonPhraseIs("some reason"))
                .BDDfy();
        }

        private void GivenThereIsAServiceRunningOn(string baseUrl, string basePath, string reasonPhrase)
        {
            _serviceHandler.GivenThereIsAServiceRunningOn(baseUrl, basePath, async context =>
            {
                context.Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = reasonPhrase;

                await context.Response.WriteAsync("YOYO!");
            });
        }

        public void Dispose()
        {
            _serviceHandler?.Dispose();
            _steps.Dispose();
        }
    }
}
