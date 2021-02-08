namespace Ocelot.AcceptanceTests
{
    using Microsoft.AspNetCore.Http;
    using Ocelot.Configuration.File;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using TestStack.BDDfy;
    using Xunit;

    public class MethodTests : IDisposable
    {
        private readonly Steps _steps;
        private readonly ServiceHandler _serviceHandler;

        public MethodTests()
        {
            _serviceHandler = new ServiceHandler();
            _steps = new Steps();
        }

        [Fact]
        public void should_return_response_200_when_get_converted_to_post()
        {
            var port = RandomPortFinder.GetRandomPort();

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                    {
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/{url}",
                            DownstreamScheme = "http",
                            UpstreamPathTemplate = "/{url}",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            DownstreamHostAndPorts = new List<FileHostAndPort>
                            {
                                new FileHostAndPort
                                {
                                    Host = "localhost",
                                    Port = port,
                                },
                            },
                            DownstreamHttpMethod = "POST",
                        },
                    },
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}/", "/", "POST"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .BDDfy();
        }

        [Fact]
        public void should_return_response_200_when_put_converted_to_post_with_content()
        {
            var port = RandomPortFinder.GetRandomPort();

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                {
                    new FileRoute
                    {
                        DownstreamPathTemplate = "/{url}",
                        DownstreamScheme = "http",
                        UpstreamPathTemplate = "/{url}",
                        UpstreamHttpMethod = new List<string> { "Put" },
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "localhost",
                                Port = port,
                            },
                        },
                        DownstreamHttpMethod = "POST",
                    },
                },
            };

            const string expected = "here is some content";
            var httpContent = new StringContent(expected);

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}/", "/", "POST"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .When(x => _steps.WhenIPutUrlOnTheApiGateway("/", httpContent))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(_ => _steps.ThenTheResponseBodyShouldBe(expected))
                .BDDfy();
        }

        [Fact]
        public void should_return_response_200_and_remove_content_when_post_with_content_converted_to_get()
        {
            var port = RandomPortFinder.GetRandomPort();

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                {
                    new FileRoute
                    {
                        DownstreamPathTemplate = "/{url}",
                        DownstreamScheme = "http",
                        UpstreamPathTemplate = "/{url}",
                        UpstreamHttpMethod = new List<string> { "Post" },
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "localhost",
                                Port = port,
                            },
                        },
                        DownstreamHttpMethod = "GET",
                    },
                },
            };

            const string expected = "here is some content";
            var httpContent = new StringContent(expected);

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}/", "/", "GET"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .When(x => _steps.WhenIPostUrlOnTheApiGateway("/", httpContent))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(_ => _steps.ThenTheContentLengthIs(0))
                .BDDfy();
        }

        private void GivenThereIsAServiceRunningOn(string baseUrl, string basePath, string expected)
        {
            _serviceHandler.GivenThereIsAServiceRunningOn(baseUrl, basePath, async context =>
            {
                if (context.Request.Method == expected)
                {
                    context.Response.StatusCode = 200;
                    var reader = new StreamReader(context.Request.Body);
                    var body = await reader.ReadToEndAsync();
                    await context.Response.WriteAsync(body);
                }
                else
                {
                    context.Response.StatusCode = 500;
                }
            });
        }

        public void Dispose()
        {
            _serviceHandler.Dispose();
            _steps.Dispose();
        }
    }
}
