namespace Ocelot.AcceptanceTests
{
    using Microsoft.AspNetCore.Http;
    using Ocelot.Configuration.File;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using TestStack.BDDfy;
    using Xunit;

    public class ServiceFabricTests : IDisposable
    {
        private readonly Steps _steps;
        private string _downstreamPath;
        private readonly ServiceHandler _serviceHandler;

        public ServiceFabricTests()
        {
            _serviceHandler = new ServiceHandler();
            _steps = new Steps();
        }

        [Fact]
        public void should_fix_issue_555()
        {
            var port = RandomPortFinder.GetRandomPort();

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                    {
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/{everything}",
                            DownstreamScheme = "http",
                            UpstreamPathTemplate = "/{everything}",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            ServiceName = "OcelotServiceApplication/OcelotApplicationService"
                        }
                    },
                GlobalConfiguration = new FileGlobalConfiguration
                {
                    ServiceDiscoveryProvider = new FileServiceDiscoveryProvider()
                    {
                        Host = "localhost",
                        Port = port,
                        Type = "ServiceFabric"
                    }
                }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/OcelotServiceApplication/OcelotApplicationService/a", 200, "Hello from Laura", "b=c"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/a?b=c"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from Laura"))
                .BDDfy();
        }

        [Fact]
        public void should_support_service_fabric_naming_and_dns_service_stateless_and_guest()
        {
            var port = RandomPortFinder.GetRandomPort();

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                    {
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/api/values",
                            DownstreamScheme = "http",
                            UpstreamPathTemplate = "/EquipmentInterfaces",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            ServiceName = "OcelotServiceApplication/OcelotApplicationService"
                        }
                    },
                GlobalConfiguration = new FileGlobalConfiguration
                {
                    ServiceDiscoveryProvider = new FileServiceDiscoveryProvider()
                    {
                        Host = "localhost",
                        Port = port,
                        Type = "ServiceFabric"
                    }
                }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/OcelotServiceApplication/OcelotApplicationService/api/values", 200, "Hello from Laura", "test=best"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/EquipmentInterfaces?test=best"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from Laura"))
                .BDDfy();
        }

        [Fact]
        public void should_support_service_fabric_naming_and_dns_service_statefull_and_actors()
        {
            var port = RandomPortFinder.GetRandomPort();

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                {
                    new FileRoute
                    {
                        DownstreamPathTemplate = "/api/values",
                        DownstreamScheme = "http",
                        UpstreamPathTemplate = "/EquipmentInterfaces",
                        UpstreamHttpMethod = new List<string> { "Get" },
                        ServiceName = "OcelotServiceApplication/OcelotApplicationService"
                    }
                },
                GlobalConfiguration = new FileGlobalConfiguration
                {
                    ServiceDiscoveryProvider = new FileServiceDiscoveryProvider()
                    {
                        Host = "localhost",
                        Port = port,
                        Type = "ServiceFabric"
                    }
                }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/OcelotServiceApplication/OcelotApplicationService/api/values", 200, "Hello from Laura", "PartitionKind=test&PartitionKey=1"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/EquipmentInterfaces?PartitionKind=test&PartitionKey=1"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from Laura"))
                .BDDfy();
        }

        [Fact]
        public void should_support_placeholder_in_service_fabric_service_name()
        {
            var port = RandomPortFinder.GetRandomPort();

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                    {
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/values",
                            DownstreamScheme = "http",
                            UpstreamPathTemplate = "/api/{version}/values",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            ServiceName = "Service_{version}/Api"
                        }
                    },
                GlobalConfiguration = new FileGlobalConfiguration
                {
                    ServiceDiscoveryProvider = new FileServiceDiscoveryProvider()
                    {
                        Host = "localhost",
                        Port = port,
                        Type = "ServiceFabric"
                    }
                }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn($"http://localhost:{port}", "/Service_1.0/Api/values", 200, "Hello from Laura", "test=best"))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunning())
                .When(x => _steps.WhenIGetUrlOnTheApiGateway("/api/1.0/values?test=best"))
                .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from Laura"))
                .BDDfy();
        }

        private void GivenThereIsAServiceRunningOn(string baseUrl, string basePath, int statusCode, string responseBody, string expectedQueryString)
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
                    if (context.Request.QueryString.Value.Contains(expectedQueryString))
                    {
                        context.Response.StatusCode = statusCode;
                        await context.Response.WriteAsync(responseBody);
                    }
                    else
                    {
                        context.Response.StatusCode = statusCode;
                        await context.Response.WriteAsync("downstream path didnt match base path");
                    }
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
