namespace Ocelot.AcceptanceTests
{
    using Microsoft.AspNetCore.Http;
    using Ocelot.Configuration.File;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using TestStack.BDDfy;
    using Xunit;

    public class ContentTests : IDisposable
    {
        private readonly Steps _steps;
        private string _contentType;
        private long? _contentLength;
        private bool _contentTypeHeaderExists;
        private readonly ServiceHandler _serviceHandler;

        public ContentTests()
        {
            _serviceHandler = new ServiceHandler();
            _steps = new Steps();
        }

        [Fact]
        public void should_not_add_content_type_or_content_length_headers()
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

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/", 200, "Hello from Laura"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from Laura"))
                .And(x => ThenTheContentTypeShouldBeEmpty())
                .And(x => ThenTheContentLengthShouldBeZero())
                .BDDfy();
        }

        [Fact]
        public void should_add_content_type_and_content_length_headers()
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
                        }
                    }
            };

            var contentType = "application/json";

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/", 201, string.Empty))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .And(x => _steps.GivenThePostHasContent("postContent"))
                .And(x => _steps.GivenThePostHasContentType(contentType))
                .When(x => _steps.WhenIPostUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.Created))
                .And(x => ThenTheContentLengthIs(11))
                .And(x => ThenTheContentTypeIsIs(contentType))
                .BDDfy();
        }

        [Fact]
        public void should_add_default_content_type_header()
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
                        }
                    }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/", 201, string.Empty))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .And(x => _steps.GivenThePostHasContent("postContent"))
                .When(x => _steps.WhenIPostUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.Created))
                .And(x => ThenTheContentLengthIs(11))
                .And(x => ThenTheContentTypeIsIs("text/plain; charset=utf-8"))
                .BDDfy();
        }

        private void ThenTheContentTypeIsIs(string expected)
        {
            _contentType.ShouldBe(expected);
        }

        private void ThenTheContentLengthShouldBeZero()
        {
            _contentLength.ShouldBeEquivalentTo(0L);
        }

        private void ThenTheContentLengthIs(int expected)
        {
            _contentLength.ShouldBe(expected);
        }

        private void ThenTheContentTypeShouldBeEmpty()
        {
            _contentType.ShouldBeNullOrEmpty();
            _contentTypeHeaderExists.ShouldBe(false);
        }

        private void GivenThereIsAServiceRunningOn(string baseUrl, string basePath, int statusCode, string responseBody)
        {
            _serviceHandler.GivenThereIsAServiceRunningOn(baseUrl, basePath, async context =>
            {
                _contentType = context.Request.ContentType;
                _contentLength = context.Request.ContentLength;
                _contentTypeHeaderExists = context.Request.Headers.TryGetValue("Content-Type", out var value);
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
