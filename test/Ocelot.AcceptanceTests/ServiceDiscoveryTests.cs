namespace Ocelot.AcceptanceTests
{
    using Configuration.File;
    using Consul;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using TestStack.BDDfy;
    using Xunit;

    public class ServiceDiscoveryTests : IDisposable
    {
        private readonly Steps _steps;
        private readonly List<ServiceEntry> _consulServices;
        private int _counterOne;
        private int _counterTwo;
        private static readonly object SyncLock = new object();
        private string _downstreamPath;
        private string _receivedToken;
        private readonly ServiceHandler _serviceHandler;

        public ServiceDiscoveryTests()
        {
            _serviceHandler = new ServiceHandler();
            _steps = new Steps();
            _consulServices = new List<ServiceEntry>();
        }

        [Fact]
        public void should_use_consul_service_discovery_and_load_balance_request()
        {
            var consulPort = RandomPortFinder.GetRandomPort();
            var servicePort1 = RandomPortFinder.GetRandomPort();
            var servicePort2 = RandomPortFinder.GetRandomPort();
            var serviceName = "product";
            var downstreamServiceOneUrl = $"http://localhost:{servicePort1}";
            var downstreamServiceTwoUrl = $"http://localhost:{servicePort2}";
            var fakeConsulServiceDiscoveryUrl = $"http://localhost:{consulPort}";
            var serviceEntryOne = new ServiceEntry()
            {
                Service = new AgentService()
                {
                    Service = serviceName,
                    Address = "localhost",
                    Port = servicePort1,
                    ID = Guid.NewGuid().ToString(),
                    Tags = new string[0]
                },
            };
            var serviceEntryTwo = new ServiceEntry()
            {
                Service = new AgentService()
                {
                    Service = serviceName,
                    Address = "localhost",
                    Port = servicePort2,
                    ID = Guid.NewGuid().ToString(),
                    Tags = new string[0]
                },
            };

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                    {
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/",
                            DownstreamScheme = "http",
                            UpstreamPathTemplate = "/",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            ServiceName = serviceName,
                            LoadBalancerOptions = new FileLoadBalancerOptions { Type = "LeastConnection" },
                        }
                    },
                GlobalConfiguration = new FileGlobalConfiguration()
                {
                    ServiceDiscoveryProvider = new FileServiceDiscoveryProvider()
                    {
                        Scheme = "http",
                        Host = "localhost",
                        Port = consulPort
                    }
                }
            };

            this.Given(x => x.GivenProductServiceOneIsRunning(downstreamServiceOneUrl, 200))
                .And(x => x.GivenProductServiceTwoIsRunning(downstreamServiceTwoUrl, 200))
                .And(x => x.GivenThereIsAFakeConsulServiceDiscoveryProvider(fakeConsulServiceDiscoveryUrl, serviceName))
                .And(x => x.GivenTheServicesAreRegisteredWithConsul(serviceEntryOne, serviceEntryTwo))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunningWithConsul())
                .When(x => _steps.WhenIGetUrlOnTheApiGatewayMultipleTimes("/", 50))
                .Then(x => x.ThenTheTwoServicesShouldHaveBeenCalledTimes(50))
                .And(x => x.ThenBothServicesCalledRealisticAmountOfTimes(24, 26))
                .BDDfy();
        }

        [Fact]
        public void should_handle_request_to_consul_for_downstream_service_and_make_request()
        {
            int consulPort = RandomPortFinder.GetRandomPort();
            int servicePort = RandomPortFinder.GetRandomPort();
            const string serviceName = "web";
            string downstreamServiceOneUrl = $"http://localhost:{servicePort}";
            var fakeConsulServiceDiscoveryUrl = $"http://localhost:{consulPort}";
            var serviceEntryOne = new ServiceEntry()
            {
                Service = new AgentService()
                {
                    Service = serviceName,
                    Address = "localhost",
                    Port = servicePort,
                    ID = "web_90_0_2_224_8080",
                    Tags = new[] { "version-v1" }
                },
            };

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                    {
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/api/home",
                            DownstreamScheme = "http",
                            UpstreamPathTemplate = "/home",
                            UpstreamHttpMethod = new List<string> { "Get", "Options" },
                            ServiceName = serviceName,
                            LoadBalancerOptions = new FileLoadBalancerOptions { Type = "LeastConnection" },
                        }
                    },
                GlobalConfiguration = new FileGlobalConfiguration()
                {
                    ServiceDiscoveryProvider = new FileServiceDiscoveryProvider()
                    {
                        Scheme = "http",
                        Host = "localhost",
                        Port = consulPort
                    }
                }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn(downstreamServiceOneUrl, "/api/home", 200, "Hello from Laura"))
            .And(x => x.GivenThereIsAFakeConsulServiceDiscoveryProvider(fakeConsulServiceDiscoveryUrl, serviceName))
            .And(x => x.GivenTheServicesAreRegisteredWithConsul(serviceEntryOne))
            .And(x => _steps.GivenThereIsAConfiguration(configuration))
            .And(x => _steps.GivenOcelotIsRunningWithConsul())
            .When(x => _steps.WhenIGetUrlOnTheApiGateway("/home"))
            .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
            .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from Laura"))
            .BDDfy();
        }

        [Fact]
        public void should_handle_request_to_consul_for_downstream_service_and_make_request_no_re_routes()
        {
            int consulPort = RandomPortFinder.GetRandomPort();
            const string serviceName = "web";
            int downstreamServicePort = RandomPortFinder.GetRandomPort();
            var downstreamServiceOneUrl = $"http://localhost:{downstreamServicePort}";
            var fakeConsulServiceDiscoveryUrl = $"http://localhost:{consulPort}";
            var serviceEntryOne = new ServiceEntry()
            {
                Service = new AgentService()
                {
                    Service = serviceName,
                    Address = "localhost",
                    Port = downstreamServicePort,
                    ID = "web_90_0_2_224_8080",
                    Tags = new[] { "version-v1" }
                },
            };

            var configuration = new FileConfiguration
            {
                GlobalConfiguration = new FileGlobalConfiguration
                {
                    ServiceDiscoveryProvider = new FileServiceDiscoveryProvider
                    {
                        Scheme = "http",
                        Host = "localhost",
                        Port = consulPort
                    },
                    DownstreamScheme = "http",
                    HttpHandlerOptions = new FileHttpHandlerOptions
                    {
                        AllowAutoRedirect = true,
                        UseCookieContainer = true,
                        UseTracing = false
                    }
                }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn(downstreamServiceOneUrl, "/something", 200, "Hello from Laura"))
            .And(x => x.GivenThereIsAFakeConsulServiceDiscoveryProvider(fakeConsulServiceDiscoveryUrl, serviceName))
            .And(x => x.GivenTheServicesAreRegisteredWithConsul(serviceEntryOne))
            .And(x => _steps.GivenThereIsAConfiguration(configuration))
            .And(x => _steps.GivenOcelotIsRunningWithConsul())
            .When(x => _steps.WhenIGetUrlOnTheApiGateway("/web/something"))
            .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
            .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from Laura"))
            .BDDfy();
        }

        [Fact]
        public void should_use_consul_service_discovery_and_load_balance_request_no_re_routes()
        {
            var consulPort = RandomPortFinder.GetRandomPort();
            var serviceName = "product";
            var serviceOnePort = RandomPortFinder.GetRandomPort();
            var serviceTwoPort = RandomPortFinder.GetRandomPort();
            var downstreamServiceOneUrl = $"http://localhost:{serviceOnePort}";
            var downstreamServiceTwoUrl = $"http://localhost:{serviceTwoPort}";
            var fakeConsulServiceDiscoveryUrl = $"http://localhost:{consulPort}";
            var serviceEntryOne = new ServiceEntry()
            {
                Service = new AgentService()
                {
                    Service = serviceName,
                    Address = "localhost",
                    Port = serviceOnePort,
                    ID = Guid.NewGuid().ToString(),
                    Tags = new string[0]
                },
            };
            var serviceEntryTwo = new ServiceEntry()
            {
                Service = new AgentService()
                {
                    Service = serviceName,
                    Address = "localhost",
                    Port = serviceTwoPort,
                    ID = Guid.NewGuid().ToString(),
                    Tags = new string[0]
                },
            };

            var configuration = new FileConfiguration
            {
                GlobalConfiguration = new FileGlobalConfiguration()
                {
                    ServiceDiscoveryProvider = new FileServiceDiscoveryProvider()
                    {
                        Scheme = "http",
                        Host = "localhost",
                        Port = consulPort
                    },
                    LoadBalancerOptions = new FileLoadBalancerOptions { Type = "LeastConnection" },
                    DownstreamScheme = "http"
                }
            };

            this.Given(x => x.GivenProductServiceOneIsRunning(downstreamServiceOneUrl, 200))
                .And(x => x.GivenProductServiceTwoIsRunning(downstreamServiceTwoUrl, 200))
                .And(x => x.GivenThereIsAFakeConsulServiceDiscoveryProvider(fakeConsulServiceDiscoveryUrl, serviceName))
                .And(x => x.GivenTheServicesAreRegisteredWithConsul(serviceEntryOne, serviceEntryTwo))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunningWithConsul())
                .When(x => _steps.WhenIGetUrlOnTheApiGatewayMultipleTimes($"/{serviceName}/", 50))
                .Then(x => x.ThenTheTwoServicesShouldHaveBeenCalledTimes(50))
                .And(x => x.ThenBothServicesCalledRealisticAmountOfTimes(24, 26))
                .BDDfy();
        }

        [Fact]
        public void should_use_token_to_make_request_to_consul()
        {
            var token = "abctoken";
            var consulPort = RandomPortFinder.GetRandomPort();
            var serviceName = "web";
            var servicePort = RandomPortFinder.GetRandomPort();
            var downstreamServiceOneUrl = $"http://localhost:{servicePort}";
            var fakeConsulServiceDiscoveryUrl = $"http://localhost:{consulPort}";
            var serviceEntryOne = new ServiceEntry()
            {
                Service = new AgentService()
                {
                    Service = serviceName,
                    Address = "localhost",
                    Port = servicePort,
                    ID = "web_90_0_2_224_8080",
                    Tags = new[] { "version-v1" }
                },
            };

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                    {
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/api/home",
                            DownstreamScheme = "http",
                            UpstreamPathTemplate = "/home",
                            UpstreamHttpMethod = new List<string> { "Get", "Options" },
                            ServiceName = serviceName,
                            LoadBalancerOptions = new FileLoadBalancerOptions { Type = "LeastConnection" },
                        }
                    },
                GlobalConfiguration = new FileGlobalConfiguration()
                {
                    ServiceDiscoveryProvider = new FileServiceDiscoveryProvider()
                    {
                        Scheme = "http",
                        Host = "localhost",
                        Port = consulPort,
                        Token = token
                    }
                }
            };

            this.Given(_ => GivenThereIsAServiceRunningOn(downstreamServiceOneUrl, "/api/home", 200, "Hello from Laura"))
                .And(_ => GivenThereIsAFakeConsulServiceDiscoveryProvider(fakeConsulServiceDiscoveryUrl, serviceName))
                .And(_ => GivenTheServicesAreRegisteredWithConsul(serviceEntryOne))
                .And(_ => _steps.GivenThereIsAConfiguration(configuration))
                .And(_ => _steps.GivenOcelotIsRunningWithConsul())
                .When(_ => _steps.WhenIGetUrlOnTheApiGateway("/home"))
                .Then(_ => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
                .And(_ => _steps.ThenTheResponseBodyShouldBe("Hello from Laura"))
                .And(_ => _receivedToken.ShouldBe(token))
                .BDDfy();
        }

        [Fact]
        public void should_send_request_to_service_after_it_becomes_available_in_consul()
        {
            var consulPort = RandomPortFinder.GetRandomPort();
            var serviceName = "product";
            var servicePort1 = RandomPortFinder.GetRandomPort();
            var servicePort2 = RandomPortFinder.GetRandomPort();
            var downstreamServiceOneUrl = $"http://localhost:{servicePort1}";
            var downstreamServiceTwoUrl = $"http://localhost:{servicePort2}";
            var fakeConsulServiceDiscoveryUrl = $"http://localhost:{consulPort}";
            var serviceEntryOne = new ServiceEntry()
            {
                Service = new AgentService()
                {
                    Service = serviceName,
                    Address = "localhost",
                    Port = servicePort1,
                    ID = Guid.NewGuid().ToString(),
                    Tags = new string[0]
                },
            };
            var serviceEntryTwo = new ServiceEntry()
            {
                Service = new AgentService()
                {
                    Service = serviceName,
                    Address = "localhost",
                    Port = servicePort2,
                    ID = Guid.NewGuid().ToString(),
                    Tags = new string[0]
                },
            };

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                    {
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/",
                            DownstreamScheme = "http",
                            UpstreamPathTemplate = "/",
                            UpstreamHttpMethod = new List<string> { "Get" },
                            ServiceName = serviceName,
                            LoadBalancerOptions = new FileLoadBalancerOptions { Type = "LeastConnection" },
                        }
                    },
                GlobalConfiguration = new FileGlobalConfiguration()
                {
                    ServiceDiscoveryProvider = new FileServiceDiscoveryProvider()
                    {
                        Scheme = "http",
                        Host = "localhost",
                        Port = consulPort
                    }
                }
            };

            this.Given(x => x.GivenProductServiceOneIsRunning(downstreamServiceOneUrl, 200))
                .And(x => x.GivenProductServiceTwoIsRunning(downstreamServiceTwoUrl, 200))
                .And(x => x.GivenThereIsAFakeConsulServiceDiscoveryProvider(fakeConsulServiceDiscoveryUrl, serviceName))
                .And(x => x.GivenTheServicesAreRegisteredWithConsul(serviceEntryOne, serviceEntryTwo))
                .And(x => _steps.GivenThereIsAConfiguration(configuration))
                .And(x => _steps.GivenOcelotIsRunningWithConsul())
                .And(x => _steps.WhenIGetUrlOnTheApiGatewayMultipleTimes("/", 10))
                .And(x => x.ThenTheTwoServicesShouldHaveBeenCalledTimes(10))
                .And(x => x.ThenBothServicesCalledRealisticAmountOfTimes(4, 6))
                .And(x => WhenIRemoveAService(serviceEntryTwo))
                .And(x => GivenIResetCounters())
                .And(x => _steps.WhenIGetUrlOnTheApiGatewayMultipleTimes("/", 10))
                .And(x => ThenOnlyOneServiceHasBeenCalled())
                .And(x => WhenIAddAServiceBackIn(serviceEntryTwo))
                .And(x => GivenIResetCounters())
                .When(x => _steps.WhenIGetUrlOnTheApiGatewayMultipleTimes("/", 10))
                .Then(x => x.ThenTheTwoServicesShouldHaveBeenCalledTimes(10))
                .And(x => x.ThenBothServicesCalledRealisticAmountOfTimes(4, 6))
                .BDDfy();
        }

        [Fact]
        public void should_handle_request_to_poll_consul_for_downstream_service_and_make_request()
        {
            int consulPort = RandomPortFinder.GetRandomPort();
            const string serviceName = "web";
            int downstreamServicePort = RandomPortFinder.GetRandomPort();
            var downstreamServiceOneUrl = $"http://localhost:{downstreamServicePort}";
            var fakeConsulServiceDiscoveryUrl = $"http://localhost:{consulPort}";
            var serviceEntryOne = new ServiceEntry()
            {
                Service = new AgentService()
                {
                    Service = serviceName,
                    Address = "localhost",
                    Port = downstreamServicePort,
                    ID = $"web_90_0_2_224_{downstreamServicePort}",
                    Tags = new[] { "version-v1" }
                },
            };

            var configuration = new FileConfiguration
            {
                Routes = new List<FileRoute>
                    {
                        new FileRoute
                        {
                            DownstreamPathTemplate = "/api/home",
                            DownstreamScheme = "http",
                            UpstreamPathTemplate = "/home",
                            UpstreamHttpMethod = new List<string> { "Get", "Options" },
                            ServiceName = serviceName,
                            LoadBalancerOptions = new FileLoadBalancerOptions { Type = "LeastConnection" },
                        }
                    },
                GlobalConfiguration = new FileGlobalConfiguration()
                {
                    ServiceDiscoveryProvider = new FileServiceDiscoveryProvider()
                    {
                        Scheme = "http",
                        Host = "localhost",
                        Port = consulPort,
                        Type = "PollConsul",
                        PollingInterval = 0,
                        Namespace = string.Empty
                    }
                }
            };

            this.Given(x => x.GivenThereIsAServiceRunningOn(downstreamServiceOneUrl, "/api/home", 200, "Hello from Laura"))
            .And(x => x.GivenThereIsAFakeConsulServiceDiscoveryProvider(fakeConsulServiceDiscoveryUrl, serviceName))
            .And(x => x.GivenTheServicesAreRegisteredWithConsul(serviceEntryOne))
            .And(x => _steps.GivenThereIsAConfiguration(configuration))
            .And(x => _steps.GivenOcelotIsRunningWithConsul())
                .When(x => _steps.WhenIGetUrlOnTheApiGatewayWaitingForTheResponseToBeOk("/home"))
            .Then(x => _steps.ThenTheStatusCodeShouldBe(HttpStatusCode.OK))
            .And(x => _steps.ThenTheResponseBodyShouldBe("Hello from Laura"))
            .BDDfy();
        }

        private void WhenIAddAServiceBackIn(ServiceEntry serviceEntryTwo)
        {
            _consulServices.Add(serviceEntryTwo);
        }

        private void ThenOnlyOneServiceHasBeenCalled()
        {
            _counterOne.ShouldBe(10);
            _counterTwo.ShouldBe(0);
        }

        private void WhenIRemoveAService(ServiceEntry serviceEntryTwo)
        {
            _consulServices.Remove(serviceEntryTwo);
        }

        private void GivenIResetCounters()
        {
            _counterOne = 0;
            _counterTwo = 0;
        }

        private void ThenBothServicesCalledRealisticAmountOfTimes(int bottom, int top)
        {
            _counterOne.ShouldBeInRange(bottom, top);
            _counterOne.ShouldBeInRange(bottom, top);
        }

        private void ThenTheTwoServicesShouldHaveBeenCalledTimes(int expected)
        {
            var total = _counterOne + _counterTwo;
            total.ShouldBe(expected);
        }

        private void GivenTheServicesAreRegisteredWithConsul(params ServiceEntry[] serviceEntries)
        {
            foreach (var serviceEntry in serviceEntries)
            {
                _consulServices.Add(serviceEntry);
            }
        }

        private void GivenThereIsAFakeConsulServiceDiscoveryProvider(string url, string serviceName)
        {
            _serviceHandler.GivenThereIsAServiceRunningOn(url, async context =>
            {
                if (context.Request.Path.Value == $"/v1/health/service/{serviceName}")
                {
                    if (context.Request.Headers.TryGetValue("X-Consul-Token", out var values))
                    {
                        _receivedToken = values.First();
                    }
                    var json = JsonConvert.SerializeObject(_consulServices);
                    context.Response.Headers.Add("Content-Type", "application/json");
                    await context.Response.WriteAsync(json);
                }
            });
        }

        private void GivenProductServiceOneIsRunning(string url, int statusCode)
        {
            _serviceHandler.GivenThereIsAServiceRunningOn(url, async context =>
            {
                try
                {
                    string response;
                    lock (SyncLock)
                    {
                        _counterOne++;
                        response = _counterOne.ToString();
                    }

                    context.Response.StatusCode = statusCode;
                    await context.Response.WriteAsync(response);
                }
                catch (Exception exception)
                {
                    await context.Response.WriteAsync(exception.StackTrace);
                }
            });
        }

        private void GivenProductServiceTwoIsRunning(string url, int statusCode)
        {
            _serviceHandler.GivenThereIsAServiceRunningOn(url, async context =>
            {
                try
                {
                    string response;
                    lock (SyncLock)
                    {
                        _counterTwo++;
                        response = _counterTwo.ToString();
                    }

                    context.Response.StatusCode = statusCode;
                    await context.Response.WriteAsync(response);
                }
                catch (Exception exception)
                {
                    await context.Response.WriteAsync(exception.StackTrace);
                }
            });
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
    }
}
