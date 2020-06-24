namespace Ocelot.AcceptanceTests
{
    using Configuration.Repository;
    using Microsoft.AspNetCore.Http;
    using Ocelot.Configuration.File;
    using Responses;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using TestStack.BDDfy;
    using Xunit;

    public class StartupTests : IDisposable
    {
        private readonly Steps _steps;
        private readonly ServiceHandler _serviceHandler;
        private string _downstreamPath;

        public StartupTests()
        {
            _serviceHandler = new ServiceHandler();
            _steps = new Steps();
        }

        [Fact]
        public void should_not_try_and_write_to_disk_on_startup_when_not_using_admin_api()
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

            var fakeRepo = new FakeFileConfigurationRepository();

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/", 200, "Hello from Laura"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunningWithBlowingUpDiskRepo(fakeRepo))
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
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

        public void Dispose()
        {
            _serviceHandler?.Dispose();
            _steps.Dispose();
        }

        private class FakeFileConfigurationRepository : IFileConfigurationRepository
        {
            public Task<Response<FileConfiguration>> Get()
            {
                throw new NotImplementedException();
            }

            public Task<Response> Set(FileConfiguration fileConfiguration)
            {
                throw new NotImplementedException();
            }
        }
    }
}
